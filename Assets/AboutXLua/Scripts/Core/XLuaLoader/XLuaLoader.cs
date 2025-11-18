using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        Hybrid            // 先 Editor，再 AA（可用于开发期热修）
    }
    
    public sealed class Options
    {
        public Mode mode = Mode.Hybrid;
        public List<string> editorRoots = new();// 编辑器根目录,默认Assets/ + 根目录
        public List<string> aaLabels = new();   // Addressables 标签
        public List<string> extensions = new() { ".lua", ".lua.txt", ".bytes" }; // 扩展名
    }
    
    // 添加缓存，避免重复加载
    private static readonly Dictionary<string, TextAsset> _luaCache = new();
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
        if (!_isPreloaded && opt.mode != Mode.EditorOnly)
        {
            await PreloadLuaScriptsAsync(opt);
            _isPreloaded = true;
        }
        
        env.AddLoader((ref string filepath) => 
        {
            string key = NormalizeModuleKey(filepath);
            byte[] bytes = null;
            
            // 1) 尝试编辑器路径
            if (opt.mode != Mode.AddressablesOnly)
            {
                TryReadFromEditor(opt, key, ref bytes);
            }
            
            // 2) 尝试缓存
            if (bytes == null && _luaCache.TryGetValue(key, out var textAsset))
            {
                bytes = textAsset.bytes;
                Debug.Log($"Cache hit: {key}");
            }
            
            if (bytes == null) 
            {
               Debug.LogWarning($"Module not found: {key}");
            }
            return bytes;
        });
        
        Debug.Log($"Registered. Mode={opt.mode}");
    }

    #endregion
    
    #region 内部方法
    
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
                        Debug.Log($"Editor hit: {path}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"File error: {path}\n{e.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 异步预加载Lua脚本
    /// </summary>
    private static async Task PreloadLuaScriptsAsync(Options opt)
    {
        var tasks = new List<Task>();
        
        foreach (var label in opt.aaLabels)
        {
            try
            {
                Debug.Log($"Preloading Lua scripts for label: {label}");
                var loadHandle = Addressables.LoadAssetsAsync<TextAsset>(label, null); // TODO: 此处需要替换为AAPackageManager的获取
                var assets = await loadHandle.Task;
                
                foreach (var asset in assets)
                {
                    if (asset != null)
                    {
                        string key = Path.GetFileNameWithoutExtension(asset.name);
                        _luaCache[key] = asset;
                        Debug.Log($"Preloaded: {key}");
                    }
                }
                
                Addressables.Release(loadHandle);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to preload label '{label}': {e}");
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
