#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

/// <summary>
/// 辅助构建数据导出, 每次热更时变动
/// </summary>
public class HelperBuildDataExporter
{
    public const string GROUP_NAME = "HelperBuildData";
    public const string AALabelsConfigAssetPath = "Assets/Build/HelperBuildData/AddressableLabelsConfig.asset";

    #region AddressableLabelsConfig
    /// <summary>
    /// 扫描并生成 SO 数据
    /// </summary>
    public static void ExportEntries()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[LabelExporter] 未找到 AddressableAssetSettings！");
            return;
        }

        // 获取或创建 Config SO
        var config = AssetDatabase.LoadAssetAtPath<AddressableLabelsConfig>(AALabelsConfigAssetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<AddressableLabelsConfig>();
            var directory = System.IO.Path.GetDirectoryName(AALabelsConfigAssetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            AssetDatabase.CreateAsset(config, AALabelsConfigAssetPath);
        }
        
        config.allEntries.Clear();
        config.keysByType.Clear();
        config.keysByLabel.Clear();
        config.labelLogicalHashes.Clear();
        
        // 临时字典用于分类
        var tempTypeDict = new Dictionary<string, List<string>>();
        var tempLabelDict = new Dictionary<string, List<string>>();
        var groupLabelToAddressList = new Dictionary<(string Group,string CombineLabel), List<string>>();
        
        // 遍历所有 Group
        foreach (var group in settings.groups)
        {
            if (group == null) continue;
            // HelperBuildData自身组也参与处理

            foreach (var entry in group.entries)
            {
                if (entry.IsFolder || string.IsNullOrEmpty(entry.address)) continue;

                string entryType = entry.labels.Count > 0 ? entry.labels.First() : "Untyped";
                string key = entry.address;
                
                var entryData = new PackageEntry
                {
                    key = entry.address,
                    Type = entryType,
                    Labels = entry.labels.ToList()
                };
                config.allEntries.Add(entryData);
                
                // 填充 Type 索引
                if (!tempTypeDict.ContainsKey(entryType)) tempTypeDict[entryType] = new List<string>();
                tempTypeDict[entryType].Add(key);
                
                // 填充单个 Label 索引 (用于 GetKeysByLabel) 
                if (entry.labels.Count == 0)
                {
                    AddToLabelDict(tempLabelDict, "Untyped", key);
                }
                else
                {
                    foreach (var label in entry.labels)
                    {
                        AddToLabelDict(tempLabelDict, label, key);
                    }
                }
                
                // 填充组合 Label (用于 LogicalHash / VersionState)
                // PackTogetherByLabel 会将相同 Label 集合的资源打在一起。
                string combinedLabelKey;
                if (entry.labels.Count == 0)
                {
                    combinedLabelKey = "untyped";
                }
                else
                {
                    // 此处维持组合 Label 的顺序
                    var labels = entry.labels.ToList();
                    // 拼接 (例如 TextAsset + LuaScripts -> "TextAssetLuaScripts")
                    // 转小写 (因为 Addressables 生成的 Bundle 文件名通常是全小写，e.g. "..._textassetluascripts_...")
                    combinedLabelKey = string.Join("", labels).ToLowerInvariant();
                }

                // 添加到用于计算 Hash 的字典
                AddToGroupLabelDict(groupLabelToAddressList, group.Name.ToLowerInvariant(), combinedLabelKey, key);
            }
        }
        
        // 加入分类列表
        foreach (var kvp in tempTypeDict)
        {
            config.keysByType.Add(new TypeToKeys { Type = kvp.Key, Keys = kvp.Value });
        }
        foreach (var kvp in tempLabelDict)
        {
            config.keysByLabel.Add(new LabelToKeys { Label = kvp.Key, Keys = kvp.Value });
        }
        foreach (var kvp in groupLabelToAddressList)
        {
            string group = kvp.Key.Group;// 小写处理后
            string combineLabels = kvp.Key.CombineLabel;// 拼接转小写后的
            List<string> keys = kvp.Value;

            // 排序确保顺序一致
            keys.Sort(StringComparer.Ordinal);

            // 拼接key
            StringBuilder sb = new StringBuilder();
            foreach (var k in keys) sb.Append(k);

            // 计算 Hash
            string logicalHash = HashGenerator.GenerateStringHash(sb.ToString());

            config.labelLogicalHashes.Add(new GroupLabelToLogicalHash 
            { 
                Group = group,
                CombineLabel = combineLabels, 
                Hash = logicalHash 
            });
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LabelExporter] 导出完成。Entries: {config.allEntries.Count}, LogicalHashes: {config.labelLogicalHashes.Count}");
    }
    
    #region 辅助方法 
    
    private static void AddToLabelDict(Dictionary<string, List<string>> dict, string label, string key)
    {
        if (!dict.ContainsKey(label)) dict[label] = new List<string>();
        dict[label].Add(key);
    }
    
    private static void AddToGroupLabelDict(Dictionary<(string, string), List<string>> dict, string group, string label, string key)
    {
        var tuple = (group, label);
        if (!dict.ContainsKey(tuple)) dict[tuple] = new List<string>();
        dict[tuple].Add(key);
    }
    
    #endregion

    /// <summary>
    /// 确保生成的配置 SO 已经被加入到 Addressable Group 中
    /// </summary>
    public static void EnsureConfigInGroup()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup(GROUP_NAME);
        if (group == null)
        {
            group = settings.CreateGroup(GROUP_NAME, false, false, true, null);
        }

        var guid = AssetDatabase.AssetPathToGUID(AALabelsConfigAssetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        
        // 确保地址简洁，方便加载
        entry.address = "AddressableLabelsConfig"; 
        
        // 自动添加一个标签（Type）
        if(!entry.labels.Contains("AddressableLabelsConfig")) entry.labels.Add("AddressableLabelsConfig");

        Debug.Log("[LabelExporter] 已确保 Config SO 进入 HelperBuildData Group。");
    }
    #endregion
}
#endif