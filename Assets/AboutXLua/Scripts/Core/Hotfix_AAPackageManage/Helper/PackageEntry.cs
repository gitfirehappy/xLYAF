using System;
using System.Collections.Generic;

/// <summary>
/// AA包索引
/// </summary>
[Serializable]
public class PackageEntry
{
    public string key;
    public string Type; // Type通过第一个Label推导
    public List<string> Labels;
}