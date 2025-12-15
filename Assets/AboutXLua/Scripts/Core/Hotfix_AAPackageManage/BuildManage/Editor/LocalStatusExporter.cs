#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

/// <summary>
/// 本地Build数据导出，只有整包构建时使用，小版本热更不需要
/// </summary>
public class LocalStatusExporter
{
    public const string BuildIndexAssetPath = Constants.BUILD_INDEX_ASSETPATH;

    private const string GROUP_NAME = Constants.LOCAL_STATUS_GROUP_NAME;
    
    #region BuildIndex
    public static void ExportBuildIndex()
    { 
        // 创建BuildIndex
        BuildIndex buildIndex = ScriptableObject.CreateInstance<BuildIndex>();
        
        buildIndex.BuildGUID = System.DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        
        buildIndex.BuildTime = System.DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        
        buildIndex.IsDebug = EditorUserBuildSettings.development;
        
        buildIndex.Platform = EditorUserBuildSettings.activeBuildTarget.ToString();
        
        // 保存到Asset
        string directoryPath = Path.GetDirectoryName(BuildIndexAssetPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        AssetDatabase.CreateAsset(buildIndex, BuildIndexAssetPath);
        AssetDatabase.SaveAssets();
        
        // 确保BuildIndex在Addressable Group中
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
        
        var guid = AssetDatabase.AssetPathToGUID(BuildIndexAssetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        
        // 确保地址简洁，方便加载
        entry.address = "BuildIndex";
        
        // 自动添加一个标签（Type）
        if (!entry.labels.Contains("BuildIndex"))
            entry.labels.Add("BuildIndex");
        
        Debug.Log($"[BuildIndexExporter] 已确保 BuildIndex 进入 {GROUP_NAME} Group。");
    }
    #endregion
}
#endif
