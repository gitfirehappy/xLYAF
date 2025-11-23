using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VersionState
{
    public string version;   // 版本号,可用于UI显示
    public string hash;     // 唯一比对标识
    public List<BundleInfo> bundles = new List<BundleInfo>(); // 所有需要的bundle列表
}

[Serializable]
public class BundleInfo
{
    public string bundleName;   // bundle 文件名
    public string hash;         // bundle 文件的hash
    public long size;           // bundle 文件大小
}