using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    
    // Group + Label -> LogicalHash
    public List<GroupLabelToLogicalHash> labelLogicalHashes = new();

    // 运行时快速查找字典 
    ///<summary> Key: "Type" -> Value: Keys </summary>
    private Dictionary<string, List<string>> _typeDict;
    
    ///<summary> Key: "Label" -> Value: Keys </summary>
    private Dictionary<string, List<string>> _labelDict;
    
    ///<summary> Key: "Group_Label" -> Value: Hash </summary> 
    private Dictionary<string, string> _labelLogicalHashDict;

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
    
    /// <summary>
    /// 获取逻辑Hash,生成version_state 时调用
    /// </summary>
    ///    /// <param name="groupName">解析文件名得到的Group部分 (通常是小写) </param>
    /// <param name="labels">解析文件名得到的Label部分 (通常是小写拼接)</param>
    public string GetLogicalHash(string groupName,string labels)
    {
        if (_labelLogicalHashDict == null) RebuildRuntimeDicts();
        // 组合键策略需与构建时一致
        string key = $"{groupName.ToLowerInvariant()}_{labels.ToLowerInvariant()}"; 
        
        return _labelLogicalHashDict.TryGetValue(key, out var hash) ? hash : string.Empty;
    }

    private void RebuildRuntimeDicts()
    {
        _typeDict = new Dictionary<string, List<string>>();
        _labelDict = new Dictionary<string, List<string>>();
        _labelLogicalHashDict = new Dictionary<string, string>();

        foreach (var item in keysByType) _typeDict[item.Type] = item.Keys;
        foreach (var item in keysByLabel) _labelDict[item.Label] = item.Keys;
        foreach (var item in labelLogicalHashes)
        {
            string key = $"{item.Group.ToLowerInvariant()}_{item.CombineLabel.ToLowerInvariant()}";
            
            // 防止重复Key报错
            if (!_labelLogicalHashDict.ContainsKey(key))
            {
                _labelLogicalHashDict.Add(key, item.Hash);
            }
        }
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
public class GroupLabelToLogicalHash
{
    public string Group;
    public string CombineLabel;    // 这个Labels是 一组 Label拼接的索引
    public string Hash;
}
