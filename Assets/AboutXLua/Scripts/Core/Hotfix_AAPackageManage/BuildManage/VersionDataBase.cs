using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 版本号存储，仅编辑器和构建时使用
/// </summary>
public class VersionDataBase : ScriptableObject
{
    public VersionNumber CurrentVersion;
    
    public string GetVersionString() =>
        $"{CurrentVersion.Major}.{CurrentVersion.Minor}.{CurrentVersion.Patch}";
}

/// <summary>
/// 版本号数据类型，整个项目统一使用
/// </summary>
[Serializable]
public class VersionNumber
{
    public int Major;
    public int Minor;
    public int Patch;
}