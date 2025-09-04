using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/ConfigConvertSetting")]
public class ConfigConvertSettings : ScriptableObject
{
    public List<ConfigConvertChannel> channels;
}

/// <summary>
/// 配置转换通道
/// </summary>
[Serializable]
public class ConfigConvertChannel
{
    public string name;
    public string inputFolder;
    public string outputFolder;
    public ConfigFormat inputFormat;
    public ConfigFormat outputFormat;
}