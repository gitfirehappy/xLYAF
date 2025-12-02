using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 版本号存储，仅编辑器和构建时使用
/// </summary>
[CreateAssetMenu(fileName = "VersionDataBase", menuName = "Build/VersionDataBase", order = 1)]
public class VersionDataBase : ScriptableObject
{
    [Header("当前版本号")]
    public VersionNumber CurrentVersion = new() { Major = 1, Minor = 0, Patch = 0 };
    
    public void IncrementVersion(bool isMajor = false, bool isMinor = false)
    {
        if (isMajor)
        {
            CurrentVersion.Major++;
            CurrentVersion.Minor = 0;
            CurrentVersion.Patch = 0;
        }
        else if (isMinor)
        {
            CurrentVersion.Minor++;
            CurrentVersion.Patch = 0;
        }
        else
        {
            CurrentVersion.Patch++;
        }
        // TODO: 当前日期构建此时增加
    }
    
    // TODO: 添加日期显示
    // TODO: 添加当前日期构建次数
    // TODO: 可添加保护机制
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
    
    public string GetVersionString() => $"{Major}.{Minor}.{Patch}";
}