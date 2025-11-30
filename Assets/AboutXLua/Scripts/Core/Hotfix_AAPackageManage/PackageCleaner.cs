using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PackageCleaner : Singleton<PackageCleaner>
{
    /// <summary>
    /// 应用更新：删除旧文件，移动新文件
    /// </summary>
    public void ApplyUpdate(List<string> filesToDelete, string tempDownloadRoot, string finalRoot)
    {
        // 1. 删除 Local 中不再需要的旧 Bundle
        string localBundleRoot = Path.Combine(finalRoot, "bundles");
        if (Directory.Exists(localBundleRoot) && filesToDelete != null)
        {
            foreach (string fileName in filesToDelete)
            {
                string fullPath = Path.Combine(localBundleRoot, fileName);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        File.Delete(fullPath);
                        Debug.Log($"[PackageCleaner] 删除过期 Bundle: {fileName}");
                    }
                    catch (Exception e) { Debug.LogWarning($"删除失败: {fullPath}\n{e}"); }
                }
            }
        }

        // 2. 将 Remote (Temp) 中的文件移动到 Local
        // 包括 bundles 文件夹和 catalog/version 文件
        MoveDirectory(tempDownloadRoot, finalRoot);
        
        Debug.Log("[PackageCleaner] 热更文件覆盖完成。");
    }

    /// <summary>
    /// 递归移动文件夹内容
    /// </summary>
    private void MoveDirectory(string sourceDir, string destDir)
    {
        if (!Directory.Exists(sourceDir)) return;
        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

        // 移动文件
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destDir, fileName);
            
            if (File.Exists(destFile)) File.Delete(destFile); // 覆盖旧的
            File.Move(file, destFile);
        }

        // 递归移动子目录 (主要是 bundles)
        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dir);
            string destSubDir = Path.Combine(destDir, dirName);
            MoveDirectory(dir, destSubDir);
        }

        // 移完后删除空的源目录
        Directory.Delete(sourceDir, true);
    }
    
    /// <summary>
    /// 大版本清理：清空所有热更内容
    /// </summary>
    public void ClearAllHotfix()
    {
        // TODO: 路径需匹配PathManager的 BuildIndex，最好传路径
        if (Directory.Exists(PathManager.HotfixRoot))
            Directory.Delete(PathManager.HotfixRoot, true);
        PathManager.EnsureDirectories();
    }
}