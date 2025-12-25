#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

/// <summary>
/// 辅助构建数据导出, 每次热更时变动
/// </summary>
public class HelperBuildDataExporter
{
    public const string GROUP_NAME = Constants.HELPER_BUILD_DATA_GROUP_NAME;
    public const string AALabelsConfigAssetPath = Constants.AA_LABELS_CONFIG_ASSETPATH;
    public const string LuaScriptsIndexAssetPath = Constants.LUA_SCRIPTS_INDEX_ASSETPATH;
    
    /// <summary>
    /// 总导出入口
    /// </summary>
    public static void ExportData()
    {
        Debug.Log("[HelperBuildData] 开始导出所有辅助构建数据...");
        ExportAddressableLabels();
        ExportLuaScriptsIndex();
        AssetDatabase.SaveAssets();
        Debug.Log("[HelperBuildData] 导出完成。");
    }
    
    /// <summary>
    /// 确保所有配置进入 AddressableGroup
    /// </summary>
    public static void EnsureExportDataInGroup()
    { 
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;
        
        var group = settings.FindGroup(GROUP_NAME);
        if (group == null)
        {
            group = settings.CreateGroup(GROUP_NAME, false, false, true, null);
        }

        // 1. AALabelsConfig
        EnsureAssetInGroup(settings, group, AALabelsConfigAssetPath, Constants.AA_LABELS_CONFIG);
        
        // 2. LuaIndex
        EnsureAssetInGroup(settings, group, LuaScriptsIndexAssetPath, Constants.LUA_SCRIPTS_INDEX);
        
        Debug.Log("[HelperBuildData] 已确保辅助数据进入 Group。");
    }
    
    private static void EnsureAssetInGroup(AddressableAssetSettings settings, AddressableAssetGroup group, string path, string address)
    {
        var guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid)) return;

        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = address;
    }
    
    #region AddressableLabelsConfig
    
    /// <summary>
    /// 导出AddressableLabelsConfig
    /// </summary>
    private static void ExportAddressableLabels()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;

        var config = GetOrCreateAsset<AddressableLabelsConfig>(AALabelsConfigAssetPath);
        
        config.allEntries.Clear();
        config.keysByType.Clear();
        config.keysByLabel.Clear();
        config.labelLogicalHashes.Clear();
        
        var tempTypeDict = new Dictionary<string, List<string>>();
        var tempLabelDict = new Dictionary<string, List<string>>();
        var groupLabelToAddressList = new Dictionary<(string Group,string CombineLabel), List<string>>();
        
        foreach (var group in settings.groups)
        {
            if (group == null) continue;

            foreach (var entry in group.entries)
            {
                if (entry.IsFolder || string.IsNullOrEmpty(entry.address)) continue;

                string entryType = entry.labels.Count > 0 ? entry.labels.First() : "Untyped";
                string key = entry.address;
                
                config.allEntries.Add(new PackageEntry
                {
                    key = entry.address,
                    Type = entryType,
                    Labels = entry.labels.ToList()
                });
                
                // Type Dict
                if (!tempTypeDict.ContainsKey(entryType)) tempTypeDict[entryType] = new List<string>();
                tempTypeDict[entryType].Add(key);
                
                // Label Dict
                if (entry.labels.Count == 0)
                {
                    AddToLabelDict(tempLabelDict, "Untyped", key);
                }
                else
                {
                    foreach (var label in entry.labels) AddToLabelDict(tempLabelDict, label, key);
                }
                
                // Combined Label (Hash)
                string combinedLabelKey = entry.labels.Count == 0 ? "untyped" : string.Join("", entry.labels).ToLowerInvariant();
                AddToGroupLabelDict(groupLabelToAddressList, group.Name.ToLowerInvariant(), combinedLabelKey, key);
            }
        }
        
        // 序列化字典到 List
        foreach (var kvp in tempTypeDict) config.keysByType.Add(new TypeToKeys { Type = kvp.Key, Keys = kvp.Value });
        foreach (var kvp in tempLabelDict) config.keysByLabel.Add(new LabelToKeys { Label = kvp.Key, Keys = kvp.Value });
        
        foreach (var kvp in groupLabelToAddressList)
        {
            var keys = kvp.Value;
            keys.Sort(StringComparer.Ordinal);
            StringBuilder sb = new StringBuilder();
            foreach (var k in keys) sb.Append(k);
            string logicalHash = HashGenerator.GenerateStringHash(sb.ToString());

            config.labelLogicalHashes.Add(new GroupLabelToLogicalHash 
            { 
                Group = kvp.Key.Group,
                CombineLabel = kvp.Key.CombineLabel, 
                Hash = logicalHash 
            });
        }
        
        EditorUtility.SetDirty(config);
        Debug.Log($"[LabelExporter] AddressableLabelsConfig 导出完成。");
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
    
    #endregion

    #region LuaScriptsIndex

    /// <summary>
    /// 导出 Lua 脚本索引
    /// </summary>
    private static void ExportLuaScriptsIndex()
    { 
        var indexSO = GetOrCreateAsset<LuaScriptsIndex>(LuaScriptsIndexAssetPath);
        indexSO.data.Clear();

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;
        
        string[] guids = AssetDatabase.FindAssets("t:LuaScriptContainer");
        
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var container = AssetDatabase.LoadAssetAtPath<LuaScriptContainer>(path);
            if (container == null) continue;

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                // 仅警告，不中断，可能该容器不需要进包
                Debug.LogWarning($"[LuaIndexExporter] Container不在Addressables中: {container.name}");
                continue; 
            }

            var entryData = new LuaScriptsIndex.ContainerEntry
            {
                containerAddress = entry.address,
                scriptNames = new List<string>()
            };

            foreach (var asset in container.luaAssets)
            {
                if (asset == null) continue;
                string scriptKey = XLuaLoader.NormalizeModuleKey(asset.name);
                entryData.scriptNames.Add(scriptKey);
            }

            indexSO.data.Add(entryData);
        }

        EditorUtility.SetDirty(indexSO);
        Debug.Log($"[LuaIndexExporter] LuaScriptsIndex 导出完成。包含 {indexSO.data.Count} 个容器。");
    }

    #endregion
    
    private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory)) System.IO.Directory.CreateDirectory(directory);
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }
}
#endif