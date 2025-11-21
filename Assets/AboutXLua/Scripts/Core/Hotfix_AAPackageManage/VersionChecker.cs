using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class VersionChecker
{
    // TODO: 需要导出bundle差异列表
    // TODO: 区分Local和Remote路径,读Local，暂存到Remote，清理时再移动到Local
    
    private static string VersionStatePath =>
        Path.Combine(PathManager.HotfixRoot, "version_state.json");
    
    /// <summary>
    /// 从本地加载版本状态（热更新包已下载后调用）
    /// </summary>
    public async Task<VersionState> LoadLocal()
    {
        if (!File.Exists(VersionStatePath)) 
            return null;
        
        string json = File.ReadAllText(VersionStatePath);
        return JsonUtility.FromJson<VersionState>(json);
    }

    /// <summary>
    /// 从已下载的远程JSON字符串解析版本状态
    /// （HotfixManager 下载好远程版本文件传入string）
    /// </summary>
    public async Task<VersionState> LoadRemote(string json)
    {
        return JsonUtility.FromJson<VersionState>(json);
    }
    
    /// <summary>
    /// 比较本地版本和远程版本
    /// </summary>
    public async Task<bool> IsDifferent(VersionState local, VersionState remote)
    {
        if (local == null) return true;

        return !string.Equals(local.hash, remote.hash, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 保存版本状态到本地
    /// </summary>
    public async Task SaveLocal(VersionState state)
    {
        string json = JsonUtility.ToJson(state, prettyPrint: true);
        File.WriteAllText(VersionStatePath, json);
    }
}
