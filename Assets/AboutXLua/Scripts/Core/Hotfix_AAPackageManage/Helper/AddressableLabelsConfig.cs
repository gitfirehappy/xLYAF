using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 构建期导出的 AA 条目配置
/// </summary>
[CreateAssetMenu(fileName = "AddressableLabelsConfig", menuName = "Addressables/MyWork/扫描导出的PackageEntries")]
public class AddressableLabelsConfig : ScriptableObject
{
    public List<PackageEntry> allEntries = new();
    
    // Type -> Keys 索引
    public List<TypeToKeys> keysByType = new();
    
    // Label -> Keys 索引
    public List<LabelToKeys> keysByLabel = new();
    
    // Label -> LogicalHash (基于组内所有Key组合计算的Hash，用于VersionCheck稳定性)
    public List<LabelToLogicalHash> labelLogicalHashes = new();

    // 运行时快速查找字典 (非序列化, OnEnable构建)
    private Dictionary<string, List<string>> _typeDict;
    private Dictionary<string, List<string>> _labelDict;

    public List<string> GetKeysByType(string type)
    {
        if (_typeDict == null) RebuildRuntimeDicts();
        return _typeDict.TryGetValue(type, out var list) ? list : new List<string>();
    }

    public List<string> GetKeysByLabel(string label)
    {
        if (_labelDict == null) RebuildRuntimeDicts();
        return _labelDict.TryGetValue(label, out var list) ? list : new List<string>();
    }

    private void RebuildRuntimeDicts()
    {
        _typeDict = new Dictionary<string, List<string>>();
        _labelDict = new Dictionary<string, List<string>>();

        foreach (var item in keysByType) _typeDict[item.Type] = item.Keys;
        foreach (var item in keysByLabel) _labelDict[item.Label] = item.Keys;
    }
}

[Serializable]
public class TypeToKeys
{
    public string Type;
    public List<string> Keys = new();
}

[Serializable]
public class LabelToKeys
{
    public string Label;
    public List<string> Keys = new();
}

[Serializable]
public class LabelToLogicalHash
{
    // TODO: List<List<Labels>> 和 Group 对应一个逻辑Hash
    public string Key;
    public string Hash;
}
