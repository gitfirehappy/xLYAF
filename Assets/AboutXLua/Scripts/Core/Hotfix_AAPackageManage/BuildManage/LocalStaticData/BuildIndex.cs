using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 包体索引，Local唯一，不会热更
/// </summary>
[CreateAssetMenu(fileName = "BuildIndex", menuName = "Build/BuildIndex")]
public class BuildIndex : ScriptableObject
{
    public const string ASSET_ADDRESS = "BuildIndex"; // AA中的地址
    
    [Header("构建唯一标识 (每次构建整包时更新)")]
    public string BuildGUID;

    [Header("构建时间")]
    public string BuildTime;

    [Header("是否为 Debug 环境")]
    public bool IsDebug;

    [Header("目标平台")]
    public string Platform;
}
