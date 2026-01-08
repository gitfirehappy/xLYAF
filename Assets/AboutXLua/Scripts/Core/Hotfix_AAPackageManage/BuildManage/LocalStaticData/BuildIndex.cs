using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 包体索引，Local唯一，不会热更
/// </summary>
[CreateAssetMenu(fileName = Constants.BUILD_INDEX, menuName = "Build/BuildIndex")]
public class BuildIndex : ScriptableObject
{
    [Header("构建唯一标识 (每次构建整包时更新)")]
    public string BuildGUID;

    [Header("构建时间")]
    public string BuildTime;

    [Header("是否为 Debug 环境")]
    public bool IsDebug;

    [Header("目标平台")]
    public string Platform;
    
    [FormerlySerializedAs("MajorVersion")] [Header("大版本号")]
    public VersionNumber Version; 
}
