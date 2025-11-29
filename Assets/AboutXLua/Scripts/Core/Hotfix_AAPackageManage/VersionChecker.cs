using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class VersionChecker
{
    /// <summary>
    /// 检查是否是大版本更新 (e.g., 1.x.x -> 2.x.x)
    /// </summary>
    public bool IsMajorUpdate(string localVerStr, string remoteVerStr)
    {
        if (string.IsNullOrEmpty(localVerStr) || string.IsNullOrEmpty(remoteVerStr)) 
            return false;

        try 
        {
            string[] localParts = localVerStr.Split('.');
            string[] remoteParts = remoteVerStr.Split('.');
            
            // 比较主版本号
            return int.Parse(remoteParts[0]) > int.Parse(localParts[0]);
        }
        catch (Exception)
        {
            Debug.LogWarning($"[VersionChecker] 检查版本差异时出错: Local:{localVerStr} Remote:{remoteVerStr}");
            return false;
        }
    }
    
    /// <summary>
    /// 计算版本差异
    /// </summary>
    /// <param name="local">本地版本信息 (可能为 null)</param>
    /// <param name="remote">远端版本信息</param>
    public VersionDiffResult CalculateDiff(VersionState local, VersionState remote)
    {
        var result = new VersionDiffResult();
        
        // 1. 如果没有本地版本，相当于全部全新下载
        if (local == null || local.bundles == null)
        {
            result.HasUpdate = true;
            result.DownloadList = remote.bundles;
            result.TotalDownloadSize = remote.bundles.Sum(b => b.size);
            return result;
        }

        // 2. 如果总 Hash 一致，无需更新
        if (string.Equals(local.hash, remote.hash, StringComparison.OrdinalIgnoreCase))
        {
            result.HasUpdate = false;
            return result;
        }

        result.HasUpdate = true;

        // 转换为字典方便快速查找 [BundleName -> Hash]
        var localDict = local.bundles.ToDictionary(b => b.bundleName, b => b.hash);
        var remoteDict = remote.bundles.ToDictionary(b => b.bundleName, b => b.hash);

        // 3. 计算需要删除的文件：在本地存在，但在远端不存在的文件
        foreach (var localBundle in local.bundles)
        {
            if (!remoteDict.ContainsKey(localBundle.bundleName))
            {
                result.DeleteList.Add(localBundle.bundleName);
            }
        }

        // 4. 计算需要下载的文件：远端新增的 OR 远端存在但Hash与本地不一致的
        foreach (var remoteBundle in remote.bundles)
        {
            // 如果本地没有，或者 Hash 不匹配，则加入下载队列
            if (!localDict.TryGetValue(remoteBundle.bundleName, out string localHash) || 
                !string.Equals(localHash, remoteBundle.hash, StringComparison.OrdinalIgnoreCase))
            {
                result.DownloadList.Add(remoteBundle);
                result.TotalDownloadSize += remoteBundle.size;
            }
        }

        return result;
    }
    
    public VersionState ParseJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JsonUtility.FromJson<VersionState>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[VersionChecker] JSON 解析失败: {e.Message}");
            return null;
        }
    }
}

/// <summary>
/// 差异比对结果
/// </summary>
public class VersionDiffResult
{
    public bool HasUpdate;
    public long TotalDownloadSize;
    public List<string> DeleteList = new(); // 需要删除的本地旧文件(相对路径)
    public List<BundleInfo> DownloadList = new(); // 需要下载的新文件
}
