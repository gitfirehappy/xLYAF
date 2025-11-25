#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class AddressableLabelExporter
{
    public const string AssetPath = "Assets/HelperBuildData/AddressableLabelsConfig.asset";
    public const string GROUP_NAME = "HelperBuildData";

    /// <summary>
    /// 核心逻辑：扫描并生成 SO 数据
    /// </summary>
    public static void ExportEntries()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[LabelExporter] 未找到 AddressableAssetSettings！");
            return;
        }

        // 1. 获取或创建 Config SO
        var config = AssetDatabase.LoadAssetAtPath<AddressableLabelsConfig>(AssetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<AddressableLabelsConfig>();
            var directory = System.IO.Path.GetDirectoryName(AssetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            AssetDatabase.CreateAsset(config, AssetPath);
        }

        // 2. 清空旧数据
        config.allEntries.Clear();

        // 3. 遍历采集
        foreach (var group in settings.groups)
        {
            if (group == null || group.Name == GROUP_NAME) continue;

            foreach (var entry in group.entries)
            {
                if (entry.IsFolder || string.IsNullOrEmpty(entry.address)) continue;

                string entryType = entry.labels.Count > 0 ? entry.labels.First() : "Untyped";

                var entryData = new PackageEntry
                {
                    key = entry.address,
                    Type = entryType,
                    Labels = entry.labels.ToList()
                };
                config.allEntries.Add(entryData);
            }
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        // AssetDatabase.Refresh(); // 暂时不刷新，交给Manager统一处理
        Debug.Log($"[LabelExporter] 导出了 {config.allEntries.Count} 个资源 Entries");
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

        var guid = AssetDatabase.AssetPathToGUID(AssetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        
        // 确保地址简洁，方便加载
        entry.address = "AddressableLabelsConfig"; 
        
        // 自动添加一个标签（Type）
        if(!entry.labels.Contains("AddressableLabelsConfig")) entry.labels.Add("AddressableLabelsConfig");

        Debug.Log("[LabelExporter] 已确保 Config SO 进入 HelperBuildData Group。");
    }
}
#endif