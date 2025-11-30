#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public class BuildIndexExporter
{
    public const string AssetPath = "Assets/LocalStaticData/BuildIndex.asset";
    public const string GROUP_NAME = "LocalStaticData";
    
    public static void ExportBuildIndex()
    { 
        // 1. 创建BuildIndex
        BuildIndex buildIndex = ScriptableObject.CreateInstance<BuildIndex>();
        
        // 2. 设置BuildGUID为当前时间戳（确保唯一性）
        buildIndex.BuildGUID = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        
        // 3. 设置构建时间
        buildIndex.BuildTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        
        // 4. 设置是否为Debug
        buildIndex.IsDebug = EditorUserBuildSettings.development;
        
        // 5. 设置目标平台
        buildIndex.Platform = EditorUserBuildSettings.activeBuildTarget.ToString();
        
        // 6. 保存到Asset
        string directoryPath = Path.GetDirectoryName(AssetPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        AssetDatabase.CreateAsset(buildIndex, AssetPath);
        AssetDatabase.SaveAssets();
        
        // 7. 确保BuildIndex在Addressable Group中
        EnsureBuildIndexInGroup();
        
        Debug.Log($"[BuildIndexExporter] BuildIndex已生成: {buildIndex.BuildGUID}, Platform: {buildIndex.Platform}");
    }

    public static void EnsureBuildIndexInGroup()
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
        entry.address = "BuildIndex";
        
        // 自动添加一个标签（Type）
        if (!entry.labels.Contains("BuildIndex"))
            entry.labels.Add("BuildIndex");
        
        Debug.Log($"[BuildIndexExporter] 已确保 BuildIndex 进入 {GROUP_NAME} Group。");
    }
}
#endif
