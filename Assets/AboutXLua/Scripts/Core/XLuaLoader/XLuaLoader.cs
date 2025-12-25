using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using XLua;

public static class XLuaLoader
{
    public enum Mode
    {
        EditorOnly, // 只读磁盘（Editor）
        AddressablesOnly, // 只读 AA
        Hybrid // 先 Editor，再 AA
    }

    public sealed class Options
    {
        public Mode mode = Mode.Hybrid;

        public List<string> editorRoots = new(); // 编辑器根目录,默认Assets/ + 根目录

        // Addressables 标签（容器的标签）
        // 多标签求交（例如：[LuaScriptsContainer, Dialogue]）
        public List<string[]> ContainersAALabels = new();
        public List<string> extensions = new() { ".lua", ".lua.txt", ".bytes" }; // 扩展名
    }

    /// <summary>
    /// 内容缓存: Lua模块名 -> 文件二进制数据
    /// </summary> 
    private static readonly Dictionary<string, byte[]> _contentCache = new();

    /// <summary>
    /// 索引缓存 :Lua模块名(normalized) -> AA资源Key(AddressableName)
    /// 参考AAPackageManager的索引缓存思路，只存string
    /// TODO: LuaScriptsIndex已有双向缓存索引
    /// </summary>
    private static readonly Dictionary<string, string> _indexCache = new();

    private static bool _isIndexBuilt = false;

    #region 对外API

    /// <summary>
    /// 初始化并注册到指定 LuaEnv 的 AddLoader
    /// </summary>
    public static async Task SetupAndRegister(LuaEnv env, Options options = null)
    {
        if (env == null) throw new ArgumentNullException(nameof(env));
        var opt = options ?? new Options();

        // 构建索引缓存
        if (!_isIndexBuilt && opt.mode != Mode.EditorOnly)
        {
            await LoadPreBuildIndex();
            _isIndexBuilt = true;
        }

        env.AddLoader((ref string filepath) =>
        {
            string key = NormalizeModuleKey(filepath);
            byte[] bytes = null;

            // 尝试内容缓存
            if (_contentCache.TryGetValue(key, out bytes))
            {
                Debug.Log($"[LuaLoader] 缓存命中: {key}");
                return bytes;
            }

            // 尝试编辑器路径
            if (opt.mode != Mode.AddressablesOnly)
            {
                bytes = TryReadFromEditor(opt, key);
                if (bytes != null) return bytes;
            }

            // 尝试通过索引缓存查询加载（懒加载 + 写入内容缓存）
            if (_indexCache.TryGetValue(key, out string aaKey))
            {
                bytes = LoadFromAddressablesSync(aaKey, key);

                if (bytes != null)
                {
                    _contentCache[key] = bytes;
                    return bytes;
                }
            }

            Debug.LogWarning($"[LuaLoader] 没有找到Lua文件: {key}");
            return null;
        });

        Debug.Log($"[LuaLoader] 注册AddLoader成功 Mode={opt.mode}");
    }

    /// <summary>
    /// 【按容器释放】
    /// 释放指定 Addressable Key (Container) 包含的所有 Lua 脚本缓存。
    /// 场景：明确知道要卸载哪个 Container 时调用。
    /// </summary>
    /// <param name="containerAAKey">LuaScriptContainer 的 Addressable Name</param>
    public static void ReleaseScriptCacheByContainer(string containerAAKey)
    {
        if (string.IsNullOrEmpty(containerAAKey)) return;

        // 找出属于该 Container 的所有 Lua 脚本 Key
        // 由于 _indexCache 是 Script -> Container 的映射，需要遍历值
        // TODO: 有构建好的反向索引后就可以优化
        var keysToRemove = new List<string>();
        foreach (var kvp in _indexCache)
        {
            if (kvp.Value == containerAAKey)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        // 从内容缓存中移除
        int removeCount = 0;
        foreach (var scriptKey in keysToRemove)
        {
            if (_contentCache.Remove(scriptKey))
            {
                removeCount++;
            }
        }

        if (removeCount > 0)
        {
            Debug.Log($"[LuaLoader] 已释放容器 [{containerAAKey}] 下的 {removeCount} 个脚本缓存。");
        }
    }

    /// <summary>
    /// 【按标签释放】
    /// 释放带有指定 Label 的所有 Container 下的 Lua 脚本缓存。
    /// 场景：退出 "Battle" 模式时，释放所有 Label 为 "Battle" 的 Lua 脚本。
    /// </summary>
    /// <param name="label">Addressable Label</param>
    public static void ReleaseScriptCacheByLabel(string label)
    {
        if (string.IsNullOrEmpty(label)) return;

        // 获取该 Label 对应的所有 Container Key
        List<string> aaKeys = AAPackageManager.Instance.GetKeysByLabel(label);

        if (aaKeys.Count == 0) return;

        // 逐个 Container 释放
        foreach (var aaKey in aaKeys)
        {
            ReleaseScriptCacheByContainer(aaKey);
        }

        Debug.Log($"[LuaLoader] 按标签 [{label}] 释放完成，涉及 {aaKeys.Count} 个容器。");
    }

    /// <summary>
    /// 清空所有脚本内容缓存
    /// 过场景、低内存警告时调用
    /// 注意：XLua 虚拟机内部的 package.loaded 依然存在，这里只是清理 loader 的缓存
    /// </summary>
    public static void ClearAllContentCache()
    {
        int count = _contentCache.Count;
        _contentCache.Clear();
        Debug.Log($"[LuaLoader] 清空所有内容缓存，清除{count} 个脚本缓存");
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 读磁盘（Editor）
    /// </summary>
    private static byte[] TryReadFromEditor(Options opt, string key)
    {
        foreach (var root in opt.editorRoots)
        {
            foreach (var ext in opt.extensions)
            {
                string path = Path.Combine(Application.dataPath, root, key + ext);
                if (File.Exists(path))
                {
                    try
                    {
                        return File.ReadAllBytes(path);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[LuaLoader] File read error: {path}\n{e.Message}");
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 加载预构建的 Lua 脚本索引
    /// </summary>
    private static async Task LoadPreBuildIndex()
    {
        try
        {
            var indexSO = await AAPackageManager.Instance.LoadAssetAsync<LuaScriptsIndex>(Constants.LUA_SCRIPTS_INDEX);

            if (indexSO != null)
            {
                // 构建双向字典
                indexSO.BuildRuntimeDics();

                // TODO: 以下几种方法中选择其中一种
                // 1. 引用 SO 中的字典，改为public
                // 2. 在LuaScriptsIndex中写查询方法
                // 3. 拷贝 LuaScriptsIndex 到此处
            }
            else
            {
                Debug.LogError("[LuaLoader] 无法加载 LuaScriptsIndex !");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LuaLoader] 加载索引异常: {e}");
        }
    }

    /// <summary>
    /// 从 Addressables 同步加载 Lua 内容
    /// </summary>
    private static byte[] LoadFromAddressablesSync(string aaKey, string scriptName)
    {
        byte[] result = null;

        // 同步加载容器
        // 这里的 LoadAssetSync 会让引用计数 +1
        var container = AAPackageManager.Instance.LoadAssetSync<LuaScriptContainer>(aaKey);

        if (container != null)
        {
            // 查找脚本
            var asset = container.luaAssets.FirstOrDefault(a => NormalizeModuleKey(a.name) == scriptName);

            if (asset != null)
            {
                // 复制数据
                // 复制一份 byte[]，因为 TextAsset 马上要跟随 Bundle 卸载
                result = asset.bytes;
            }

            // 立即卸载容器
            // 因为已经拿到了 bytes 并准备存入 _contentCache
            // 所以不需要保持 Bundle 加载状态，节省 Native Memory
            AAPackageManager.Instance.UnloadAsset(aaKey);
        }

        return result;
    }

    #region 小工具

    /// <summary>
    /// 标准化模块名
    /// TODO: 是否需要将这种通用工具单独抽取出来？
    /// </summary>
    public static string NormalizeModuleKey(string filepath)
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