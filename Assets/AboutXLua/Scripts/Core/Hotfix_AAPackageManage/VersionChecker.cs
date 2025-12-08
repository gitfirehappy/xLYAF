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
    public bool IsMajorUpdate(VersionNumber localVerStr, VersionNumber remoteVerStr)
    {
        if (localVerStr == null || remoteVerStr == null) 
            return false;

        try 
        {
            // 比较主版本号
            return localVerStr.Major != remoteVerStr.Major;
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
        
        // 如果没有本地版本，相当于全部全新下载
        if (local == null || local.bundles == null)
        {
            result.HasUpdate = true;
            result.DownloadList = remote.bundles;
            result.TotalDownloadSize = remote.bundles.Sum(b => b.size);
            return result;
        }

        // 如果总 Hash 一致，无需更新
        if (string.Equals(local.hash, remote.hash, StringComparison.OrdinalIgnoreCase))
        {
            result.HasUpdate = false;
            return result;
        }

        result.HasUpdate = true;

        var localMap = CreateBundleMap(local.bundles);
        var remoteMap = CreateBundleMap(remote.bundles);

        // 计算需要删除的文件：在本地存在，但在远端不存在的文件
        foreach (var kvp in localMap)
        {
            if (!remoteMap.ContainsKey(kvp.Key))
            {
                // 将物理文件名添加到删除列表
                result.DeleteList.Add(kvp.Value.bundleName);
            }
        }

        // 找出需要下载的 (Remote 新增 OR Remote 存在但 logicalKey 不同)
        foreach (var kvp in remoteMap)
        {
            string logicalId = kvp.Key;
            BundleInfo remoteBundle = kvp.Value;

            if (localMap.TryGetValue(logicalId, out BundleInfo localBundle))
            {
                if (localBundle.hash != remoteBundle.hash)
                {
                    // logicalKey 变了 -> 需要更新
                    // 旧的物理文件 (localBundle.bundleName) 需要被标记删除
                    if (localBundle.bundleName != remoteBundle.bundleName)
                    {
                        if (!result.DeleteList.Contains(localBundle.bundleName))
                            result.DeleteList.Add(localBundle.bundleName);
                    }
                    
                    result.DownloadList.Add(remoteBundle);
                    result.TotalDownloadSize += remoteBundle.size;
                }
            }
            else
            {
                // 本地没有这个逻辑包 -> 新增
                result.DownloadList.Add(remoteBundle);
                result.TotalDownloadSize += remoteBundle.size;
            }
        }

        return result;
    }
    
    private Dictionary<string, BundleInfo> CreateBundleMap(List<BundleInfo> bundles)
    {
        var dict = new Dictionary<string, BundleInfo>();
        foreach (var b in bundles)
        {
            // 优先使用 logicalKey，如果无效则使用 bundleName
            string key = (!string.IsNullOrEmpty(b.logicalKey) && b.logicalKey != "Unknown") 
                ? b.logicalKey 
                : b.bundleName;
            
            if(!dict.ContainsKey(key)) dict.Add(key, b);
        }
        return dict;
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
