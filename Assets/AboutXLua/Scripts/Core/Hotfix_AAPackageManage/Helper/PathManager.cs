using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PathManager
{
    public static readonly string Root =
        Path.Combine(Application.persistentDataPath, "CFY_FrameWork");

    public static readonly string HotfixRoot =
        Path.Combine(Root, "Hotfix");

    public static readonly string LocalRoot =
        Path.Combine(HotfixRoot, "Local");
    
    public static readonly string RemoteRoot =
        Path.Combine(HotfixRoot, "Remote");
    
    public static readonly string LocalBundleRoot =
        Path.Combine(LocalRoot, "bundles");
    
    public static readonly string RemoteBundleRoot =
        Path.Combine(RemoteRoot, "bundles");

    public static readonly string CacheRoot =
        Path.Combine(Root, "Cache");

    public static readonly string SaveRoot =
        Path.Combine(Root, "Saves");

    public static readonly string LogRoot =
        Path.Combine(Root, "Logs");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(Root);
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

