using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PathManager
{
    public static readonly string PersistentRoot = Path.Combine(Application.persistentDataPath, "ProjectName");
    
    // 运行时动态决定的路径
    public static string HotfixRoot { get; private set; } // .../Hotfix
    public static string EnvRoot { get; private set; }    // .../Hotfix/[Platform]/[Debug]
    public static string CurrentGUIDRoot { get; private set; } // .../Hotfix/[Platform]/[Debug]/[GUID]
   
    public static string LocalRoot { get; private set; }
    public static string RemoteRoot { get; private set; }
    
    public static string LocalBundleRoot { get; private set; }
    public static string RemoteBundleRoot { get; private set; }
    
    public static string CacheRoot { get; private set; }
    public static string SaveRoot { get; private set; }
    public static string LogRoot { get; private set; }

    /// <summary>
    /// 初始化路径
    /// </summary>
    public static void Initialize(BuildIndex buildIndex)
    {
        string platform = buildIndex.Platform;
        if(string.IsNullOrEmpty(platform)) platform = "Unknown";

        string envDir = buildIndex.IsDebug ? "Debug" : "Release";
        string guidDir = "Build_" + buildIndex.BuildGUID;

        // 1. 组装路径结构
        // .../ProjectName/Hotfix
        HotfixRoot = Path.Combine(PersistentRoot, "Hotfix"); 
        
        // .../ProjectName/Hotfix/[Platform]/Release
        EnvRoot = Path.Combine(HotfixRoot, platform, envDir); 
        
        // .../ProjectName/Hotfix/[Platform]/Release/abc-123-guid (当前生效目录)
        CurrentGUIDRoot = Path.Combine(EnvRoot, guidDir);
        
        // 4. 定位 Local 和 Remote
        LocalRoot = Path.Combine(CurrentGUIDRoot, "Local");
        RemoteRoot = Path.Combine(CurrentGUIDRoot, "Remote");
        
        LocalBundleRoot = Path.Combine(LocalRoot, "bundles");
        RemoteBundleRoot = Path.Combine(RemoteRoot, "bundles");
        
        CacheRoot = Path.Combine(CurrentGUIDRoot, "Cache");
        SaveRoot = Path.Combine(CurrentGUIDRoot, "Saves");
        LogRoot = Path.Combine(CurrentGUIDRoot, "Logs");
        
        Debug.Log($"[PathManager] 路径已锁定至 GUID: {guidDir}\nRoot: {CurrentGUIDRoot}");
    }

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(PersistentRoot);
        Directory.CreateDirectory(HotfixRoot);
        Directory.CreateDirectory(LocalRoot);
        Directory.CreateDirectory(RemoteRoot);
        Directory.CreateDirectory(LocalBundleRoot);
        Directory.CreateDirectory(RemoteBundleRoot);
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(SaveRoot);
        Directory.CreateDirectory(LogRoot);
    }
}

