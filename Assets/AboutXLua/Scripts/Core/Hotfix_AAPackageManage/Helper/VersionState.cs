using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VersionState
{
    public float version;   // 版本号,可用于UI显示
    public string hash;     // 唯一比对标识
}
