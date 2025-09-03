using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/ConfigConvertSetting")]
public class ConfigConvertSettings : ScriptableObject
{
    public List<ConfigConvertChannel> channels;
}

[Serializable]
public class ConfigConvertChannel
{
    public string name;
    public string inputFolder;
    public string outputFolder;
    public string inputFormat;
    public string outputFormat;// e.g. "lua", "json"
}