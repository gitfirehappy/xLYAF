using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PathManager
{
    public static readonly string Root =
        Path.Combine(Application.persistentDataPath, "MyProudWork");

    public static readonly string HotfixRoot =
        Path.Combine(Root, "Hotfix");

    public static readonly string BundleRoot =
        Path.Combine(HotfixRoot, "bundles");

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
        Directory.CreateDirectory(BundleRoot);
        Directory.CreateDirectory(CacheRoot);
        Directory.CreateDirectory(SaveRoot);
        Directory.CreateDirectory(LogRoot);
    }
}

