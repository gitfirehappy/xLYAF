using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AboutXLua.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using XLua;

public static class XLuaLoader
{
    public enum Mode
    {
        EditorOnly,       // 只读磁盘（Editor）
        AddressablesOnly, // 只读 AA
        Hybrid            // 先 Editor，再 AA
    }
    
    public sealed class Options
    {
        public Mode mode = Mode.Hybrid;
        public List<string> editorRoots = new();// 编辑器根目录,默认Assets/ + 根目录
        public List<string> aaLabels = new();   // Addressables 标签（这个采用容器的标签）
        public List<string> extensions = new() { ".lua", ".lua.txt", ".bytes" }; // 扩展名
    }
    
    // 内容缓存
    private static readonly Dictionary<string, TextAsset> _luaCache = new();
    
    // 索引缓存
    // TODO: 参考AAPackageManager的索引缓存思路，只存string
    // 例如： DialogueController(ModuleName) -> Dialogue(LuaScriptsContainer的AA资源索引)
    private static readonly Dictionary<string, string> _moduleToAAKeyCache = new();
    
    private static bool _isPreloaded = false;
    
    #region 对外API

    /// <summary>
    /// 初始化并注册到指定 LuaEnv 的 AddLoader
    /// </summary>
    public static async Task SetupAndRegister(LuaEnv env, Options options = null)
    {
        if (env == null) throw new ArgumentNullException(nameof(env));
        var opt = options ?? new Options();
        
        // 预加载所有Lua脚本到缓存
        // TODO: 需要时按需加载可节省内存，不要缓存所有
        if (!_isPreloaded && opt.mode != Mode.EditorOnly)
        {
            // TODO: 先建立索引映射（不加载实际资源）
            await PreloadLuaScriptsAsync(opt);
            _isPreloaded = true;
        }
        
        env.AddLoader((ref string filepath) => 
        {
            string key = NormalizeModuleKey(filepath);
            byte[] bytes = null;
            
            // 尝试编辑器路径
            if (opt.mode != Mode.AddressablesOnly)
            {
                TryReadFromEditor(opt, key, ref bytes);
            }
            
            // 尝试缓存
            if (bytes == null && _luaCache.TryGetValue(key, out var textAsset))
            {
                bytes = textAsset.bytes;
                Debug.Log($"[LuaLoader] 缓存命中: {key}");
            }
            
            // TODO：如果没有需要尝试 Addressables 按需同步加载 (Lazy Load)
            
            if (bytes == null) 
            {
               Debug.LogWarning($"[LuaLoader] Module not found: {key}");
            }
            return bytes;
        });
        
        Debug.Log($"[LuaLoader] 注册AddLoader成功 Mode={opt.mode}");
    }
    
    // TODO: 手动加载某些脚本（可用于启动时调用）

    #endregion
    
    #region 内部方法
    // TODO: 建立映射
    // TODO: 从AA 同步加载
    
    /// <summary>
    /// 读磁盘（Editor）
    /// </summary>
    private static void TryReadFromEditor(Options opt, string key, ref byte[] bytes)
    {
        foreach (var root in opt.editorRoots)
        {
            foreach (var ext in opt.extensions)
            {
                string path = Path.Combine(Application.dataPath, root, key + ext);
                try
                {
                    if (File.Exists(path))
                    {
                        bytes = File.ReadAllBytes(path);
                        Debug.Log($"[LuaLoader] Editor hit: {path}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LuaLoader] File error: {path}\n{e.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 异步预加载Lua脚本
    /// TODO: 不需要预加载全部脚本
    /// </summary>
    private static async Task PreloadLuaScriptsAsync(Options opt)
    {
        var tasks = new List<Task>();
        
        foreach (var label in opt.aaLabels)
        {
            try
            {
                Debug.Log($"[LuaLoader] 根据标签预加载Lua脚本: {label}");
                // 通过 AAPackageManager 获取
                List<LuaScriptContainer> containers = await AAPackageManager.Instance.LoadAssetByLabelAsync<LuaScriptContainer>(label);

                foreach (var container in containers)
                {
                    if(container == null) continue;
                    
                    foreach (var asset in container.luaAssets)
                    {
                        if (asset == null) continue;
                        string key = Path.GetFileNameWithoutExtension(asset.name);
                        _luaCache[key] = asset;
                        Debug.Log($"[LuaLoader] Preloaded: {key}");
                    }
                }
                // 已缓存脚本，是否要Release AA资源的 handle
            }
            catch (Exception e)
            {
                Debug.LogError($"[LuaLoader] 加载标签失败：'{label}': {e}");
            }
        }
        
        await Task.WhenAll(tasks);
    }

    #region 小工具
    
    /// <summary>
    /// 标准化模块名
    /// </summary>
    private static string NormalizeModuleKey(string filepath)
    {
        if (string.IsNullOrEmpty(filepath)) return string.Empty;
        
        // 统一路径格式
        string key = filepath.Replace('\\', '/');
        
        // 移除扩展名
        foreach (var ext in new[] { ".lua", ".lua.txt", ".bytes" })
        {
            if (key.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring(0, key.Length - ext.Length);
                break;
            }
        }
        
        // 转换点路径为目录路径
        return key.Replace('.', '/');
    }
    
    /// <summary>
    /// 检查资源位置是否匹配模块key
    /// </summary>
    private static bool LocationMatches(IResourceLocation loc, string key, List<string> extensions)
    {
        string primaryKey = loc.PrimaryKey.Replace('\\', '/');
        string normalizedKey = key.Replace('.', '/');
        
        // 1. 文件名精确匹配（不含扩展名）
        string fileName = Path.GetFileNameWithoutExtension(primaryKey);
        if (string.Equals(fileName, normalizedKey, StringComparison.OrdinalIgnoreCase))
            return true;
        
        // 2. 完整路径匹配
        foreach (var ext in extensions)
        {
            if (primaryKey.EndsWith(normalizedKey + ext, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        // 3. 路径包含匹配
        string searchPattern = "/" + normalizedKey + ".";
        return primaryKey.IndexOf(searchPattern, StringComparison.OrdinalIgnoreCase) >= 0;
    }
    #endregion
    #endregion
    
}
