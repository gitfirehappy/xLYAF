using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VersionState
{
    public VersionNumber version;  // 版本号,可用于UI显示 
    public string hash;     // 唯一比对标识
    public long totalSize;  // 所有bundle的总大小
    public List<BundleInfo> bundles = new List<BundleInfo>(); // 所有需要的bundle列表
}

[Serializable]
public class BundleInfo
{
    public string bundleName;   // bundle 文件名（e.g group_assets_label_hash.bundle）
    public string hash;         // bundle 文件的 hash
    public string logicalKey;     // 用于差异对比，基于bundle内的key生成
    public long size;           // bundle 文件大小
}