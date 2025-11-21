using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 负责清理自定义热更目录中的历史文件，只保留当前版本所需文件
/// </summary>
public class PackageCleaner : Singleton<PackageCleaner>
{
    // TODO: 需要根据VersionChecker生成的差异bundle列表删除Local里的文件
    
    /// <summary>
    /// 删除热更目录中所有不在 validFiles 列表中的文件
    /// validFiles 全部由 HotfixManager -> VersionChecker -> CatalogUpdater 提供
    /// </summary>
    public void CleanByManifest(IReadOnlyList<string> validFiles)
    {
        string root = PathManager.LocalBundleRoot;
        if (!Directory.Exists(root))
        {
            Debug.LogWarning($"[PackageCleaner] 路径不存在: {root}");
            return;
        }

        Debug.Log($"[PackageCleaner] 开始清理旧文件... Root: {root}");

        // 把当前版本所有文件放入 Set，方便判断
        HashSet<string> validSet = new HashSet<string>(validFiles);

        // 遍历目录全部文件
        var allFiles = Directory.GetFiles(root, "*", SearchOption.AllDirectories);

        int deleteCount = 0;

        foreach (string filePath in allFiles)
        {
            string relative = Path.GetRelativePath(root, filePath);

            // version_state.json、catalog.json 如果没有在 validFiles 内，也会被删
            // 旧版本的 metadata 不应该留着
            if (!validSet.Contains(relative))
            {
                try
                {
                    File.Delete(filePath);
                    deleteCount++;
                    Debug.Log($"[PackageCleaner] 删除旧文件: {relative}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[PackageCleaner] 删除失败: {relative} \n{e}");
                }
            }
        }

        CleanupEmptyDirectories(root);

        Debug.Log($"[PackageCleaner] 清理完毕，删除 {deleteCount} 个文件。");
    }


    /// <summary>
    /// 清理空目录
    /// </summary>
    private void CleanupEmptyDirectories(string root)
    {
        foreach (string dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
        {
            if (Directory.GetFiles(dir).Length == 0 &&
                Directory.GetDirectories(dir).Length == 0)
            {
                Directory.Delete(dir);
            }
        }
    }
}
