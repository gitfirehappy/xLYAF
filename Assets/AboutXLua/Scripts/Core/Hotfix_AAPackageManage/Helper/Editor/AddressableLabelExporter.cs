#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

// TODO: 将功能调用拆分，只留下方法调用，统一由 BuildProjectManager 调度
public class AddressableLabelExporter : IPreprocessBuildWithReport
{
    // 这是 ExportEntries() 的 "输出" 这个文件在项目中的 "物理" 保存路径
    private const string AssetPath = "Assets/HelperBuildData/AddressableLabelsConfig.asset"; 
    private const string GROUP_TO_IGNORE = "HelperBuildData"; // AAPackageManager 构建时忽略的 Group
    
    // IPreprocessBuildWithReport 的接口实现
    public int callbackOrder => 0; // 排序，0表示最早执行
    
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("[AddressableLabelExporter] 触发 PreprocessBuild，开始导出 Entries...");
        ExportEntries();
    }

    [MenuItem("Tools/Addressables/Export Entries and Labels")] // 手动触发
    public static void ExportEntries()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("[LabelExporter] 未找到 AddressableAssetSettings！");
            return;
        }

        // 1. 获取或创建 Config SO（在AssetPath路径）
        var config = AssetDatabase.LoadAssetAtPath<AddressablePackagesEntries>(AssetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<AddressablePackagesEntries>();
            // 确保目录存在
            var directory = System.IO.Path.GetDirectoryName(AssetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            AssetDatabase.CreateAsset(config, AssetPath);
        }

        // 2. 清空旧数据
        config.allEntries.Clear();

        // 3. 遍历所有 Group 和 Entries
        foreach (var group in settings.groups)
        {
            if (group == null || group.Name == GROUP_TO_IGNORE)
            {
                continue;
            }

            foreach (var entry in group.entries)
            {
                // 跳过文件夹引用和没有 Key 的
                if (entry.IsFolder || string.IsNullOrEmpty(entry.address))
                {
                    continue;
                }
                
                // Type 从 Label[0] 推导 
                string entryType;
                if (entry.labels.Count > 0)
                {
                    entryType = entry.labels.First(); // 使用第一个 label 作为 Type
                }
                else
                {
                    entryType = "Untyped"; // 没有 label 的资源
                }

                var entryData = new PackageEntry
                {
                    key = entry.address,
                    Type = entryType,
                    Labels = entry.labels.ToList()
                };
                
                config.allEntries.Add(entryData);
            }
        }

        // 4. 保存资源
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LabelExporter] 导出了 {config.allEntries.Count} 个资源 Entries 到 {AssetPath}");
    }
}
#endif