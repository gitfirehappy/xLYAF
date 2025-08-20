using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor.Build.Pipeline.Interfaces;
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
        public bool log = true; // 是否输出日志
    }
    
    #region 对外API

    /// <summary>
    /// 初始化并注册到指定 LuaEnv 的 AddLoader
    /// </summary>
    public static void SetupAndRegister(LuaEnv env, Options options = null)
    {
        if (env == null) throw new ArgumentNullException(nameof(env));
        var opt = options ?? new Options();
        
        // 创建精简的Loader函数
        env.AddLoader((ref string filepath) => 
        {
            string key = NormalizeModuleKey(filepath);
            byte[] bytes = null;
            
            // 1) 尝试编辑器路径
            if (opt.mode != Mode.AddressablesOnly)
            {
                TryReadFromEditor(opt, key, ref bytes);
            }
            
            // 2) 尝试Addressables
            if (bytes == null && opt.mode != Mode.EditorOnly)
            {
                TryReadFromAddressables(opt, key, ref bytes);
            }
            
            if (bytes == null && opt.log) 
            {
               LogUtility.Warning(LogLayer.Core, "XLuaLoader", 
                   $"Module not found: {key}");
            }
            return bytes;
        });
        
        if (opt.log) LogUtility.Info(LogLayer.Core, "XLuaLoader", 
            $"Registered. Mode={opt.mode}");
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
                        if (opt.log)
                        {
                            LogUtility.Info(LogLayer.Core, "XLuaLoader", 
                                $"Editor hit: {path}");
                        }
                        return;
                    }
                }
                catch (Exception e)
                {
                    LogUtility.Error(LogLayer.Core, "XLuaLoader", 
                        $"File error: {path}\n{e.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 读 Addressables（同步）-- 纯标签扫描方案
    /// </summary>
    private static void TryReadFromAddressables(Options opt, string key, ref byte[] bytes)
    {
        // 标签扫描
        if (opt.aaLabels.Count > 0)
        {
            foreach (var label in opt.aaLabels)
            {
                try
                {
                    var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(TextAsset));
                    var locs = locHandle.WaitForCompletion();
                    
                    if (locs != null)
                    {
                        foreach (var loc in locs)
                        {
                            if (LocationMatches(loc, key, opt.extensions))
                            {
                                var assetHandle = Addressables.LoadAssetAsync<TextAsset>(loc);
                                var asset = assetHandle.WaitForCompletion();
                                
                                if (asset != null)
                                {
                                    bytes = asset.bytes;
                                    if (opt.log)
                                    {
                                        LogUtility.Info(LogLayer.Core, "XLuaLoader", 
                                            $"AA hit (label={label}): {loc.PrimaryKey}");
                                    }
                                    Addressables.Release(assetHandle);
                                    Addressables.Release(locHandle);
                                    return;
                                }
                                Addressables.Release(assetHandle);
                            }
                        }
                    }
                    Addressables.Release(locHandle);
                }
                catch (Exception e)
                {
                    if (opt.log)
                    {
                        LogUtility.Error(LogLayer.Core, "XLuaLoader", 
                            $"AA label scan failed for '{label}': {e.Message}");
                    }
                }
            }
        }
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
