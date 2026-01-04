#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>
/// 差异化构建处理器
/// 负责计算差异、临时切换Remote状态、快照轮转
/// </summary>
public static class DifferentialProcessor
{
    private static string SnapShotAssetPath = Constants.SNAPSHOT_ASSET_PATH;

    ///<summary> 缓存用于构建后还原组，GUID ->原Group </summary> 
    private static Dictionary<string, string> _groupCache = new();
    
    /// <summary>
    /// 分析快照差异并临时修改配置
    /// </summary>
    /// <param name="modifiedAssets">修改的资源列表，用于生成差异资源索引</param>
    public static bool PrepareHotfix(VersionNumber currentVersion, out List<AssetSnapshot> modifiedAssets)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var data = GetOrCreateSnapshotData();
        var head = data.GetHead();

        var currentAssets = ScanCurrentProjectAssets(settings);

        if (head == null)
        {
            Debug.LogError("[DiffProcessor] 没有找到基准版本(Head)，无法执行热更构建。请先执行 Build Full Package。");
            modifiedAssets = null;
            return false;
        }
        
        modifiedAssets = FindModifiedAssets(currentAssets, head);
        
        // 移动逻辑
        
        // 生成暂存快照
    }

    /// <summary>
    /// 恢复修改的配置
    /// </summary>
    public static void RestoreAfterHotfix()
    {
        
    }

    /// <summary>
    /// 确认发布上线，更新快照列表和 Head
    /// </summary>
    public static void ConfirmRelease()
    {
        
    }

    /// <summary>
    /// [整包构建] 生成全新的快照列表
    /// </summary>
    public static void ReBuildSnapShots()
    {
        
    }

    #region 内部辅助方法

    /// <summary>
    /// 获取快照数据
    /// </summary>
    private static BuildSnapshots GetOrCreateSnapshotData()
    {
        var data = AssetDatabase.LoadAssetAtPath<BuildSnapshots>(SnapShotAssetPath);
        if (data == null)
        {
            // 确保目录存在
            string dir = Path.GetDirectoryName(SnapShotAssetPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            data = ScriptableObject.CreateInstance<BuildSnapshots>();
            AssetDatabase.CreateAsset(data, SnapShotAssetPath);
            AssetDatabase.SaveAssets();
        }
        return data;
    }
    
    /// <summary>
    /// 扫描当前项目所有资源
    /// </summary>
    private static List<AssetSnapshot> ScanCurrentProjectAssets(AddressableAssetSettings settings)
    {
        List<AssetSnapshot> list = new List<AssetSnapshot>();
        
        foreach (var group in settings.groups)
        {
            if (group == null) continue;
            // 跳过内置数据、HelperData(由BuildManager处理)、以及我们的临时热更组
            if (group.Name == "Built In Data" || 
                group.Name == Constants.HELPER_BUILD_DATA_GROUP_NAME ||
                group.Name == Constants.HOTFIX_GROUP_NAME) 
                continue;

            foreach (var entry in group.entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.AssetPath)) continue;
                
                // 计算 Hash: 结合 GUID 和 文件修改时间/内容Hash
                string fullPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (!File.Exists(fullPath)) continue;
                
                string hash = HashGenerator.GenerateFileHash(fullPath); 
                long size = new FileInfo(fullPath).Length;

                list.Add(new AssetSnapshot
                {
                    Address = entry.address,
                    AssetPath = entry.AssetPath,
                    AssetGUID = entry.guid,
                    Labels = new List<string>(entry.labels),
                    GroupName = group.Name, // 记录其所在的本地组
                    FileHash = hash,
                    FileSize = size
                });
            }
        }
        return list;
    }

    /// <summary>
    /// 找出修改的资源
    /// </summary>
    private static List<AssetSnapshot> FindModifiedAssets(List<AssetSnapshot> currentAssets, BuildSnapshot lastSnapshot)
    {
        
    }
    
    /// <summary>
    /// 获取或创建 Hotfix 组
    /// </summary>
    private static AddressableAssetGroup GetOrCreateHotfixGroup(AddressableAssetSettings settings)
    {
        var group = settings.FindGroup(Constants.HOTFIX_GROUP_NAME);
        if (group == null)
        {
            group = settings.CreateGroup(Constants.HOTFIX_GROUP_NAME, false, false, true, null);
            
            // 添加 BundledSchema 并强制设置为 Remote
            var schema = group.AddSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel; // 或 PackTogether
        }
        return group;
    }
    
    #endregion
}
#endif