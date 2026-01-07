#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 构建快照数据结构
/// 用于记录某次构建时的资源分组状态、Hash等信息
/// </summary>
[Serializable]
public class BuildSnapshots : ScriptableObject
{
    public int HeadIndex = -1;
    
    // 历史版本快照
    public List<BuildSnapshot> Snapshots = new();

    // 暂存的快照（构建完热更包但未发布）
    public BuildSnapshot StageSnapshot;
    
    public BuildSnapshot GetHead()
    {
        if (HeadIndex >= 0 && HeadIndex < Snapshots.Count)
        {
            return Snapshots[HeadIndex];
        }
        return null;
    }
}

[Serializable]
public class BuildSnapshot
{
    public VersionNumber Version;
    public string Timestamp;
    public List<AssetSnapshot> Assets = new();
    public List<string> DeleteList = new();     // 此次构建删除的资源

    public BuildSnapshot(VersionNumber version)
    {
        Version = version;
        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

[Serializable]
public class AssetSnapshot
{
    public string Address;
    public string AssetPath;
    public string AssetGUID;
    public string GroupName;
    public List<string> Labels;
    public string FileHash;  // 内容Hash
    public bool hasUpdated; // 是否更新过
    public string RemoteGroupName; // 临时保存的远程组名
}

#endif