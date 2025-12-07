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
        var bundleStructure = new Dictionary<(string Group, string Label), List<string>>();
        
        foreach (var group in settings.groups)
        {
            if (group == null || group.Name == GROUP_NAME) continue;

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
                
                // By Type
                if (!tempTypeDict.ContainsKey(entryType)) tempTypeDict[entryType] = new List<string>();
                tempTypeDict[entryType].Add(key);
                
                // By Label
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
        
        // TODO: 根据 同Group 同Label 的资源的key 生成logicalKey

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"[LabelExporter] 导出了 {config.allEntries.Count} 个资源 Entries");
    }
    
    private static void AddToLabelDict(Dictionary<string, List<string>> dict, string label, string key)
    {
        if (!dict.ContainsKey(label)) dict[label] = new List<string>();
        dict[label].Add(key);
    }

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