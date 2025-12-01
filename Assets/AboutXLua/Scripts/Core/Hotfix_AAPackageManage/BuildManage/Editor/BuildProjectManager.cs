#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class BuildProjectManager
{
    // 项目名称
    private static string ProjectName => "ProjectName";
    
    // 热更包输出根目录
    private static string OutputRoot => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "HotfixOutput");
    
    // 热更包体大小限制
    private static long MaxHotfixSizeBytes = 1L * 1024 * 1024 * 1024;
    
    private static string versionDataBasePath => "Assets/Build/VersionDataBase.asset";

    private enum BuildType
    {
        Full,
        Hotfix
    }
    
    /// <summary>
    /// 构建完整包，用于大版本更新
    /// </summary>
    [MenuItem("Tools/Build/Build Full Package")]
    public static void BuildFullPackage()
    {
        VersionDataBase versionData = LoadVersionDataBase();
        if (versionData == null) return;
        
        // 大版本更新，增加Major版本
        versionData.IncrementVersion(true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        ExecuteBuildFlow(versionData.CurrentVersion, BuildType.Full);
    }
    
    /// <summary>
    /// 构建热更包，用于小版本更新
    /// </summary>
    [MenuItem("Tools/Build/Build Hotfix Package")]
    public static void BuildHotfix()
    {
        VersionDataBase versionData = LoadVersionDataBase();
        if (versionData == null) return;
        
        // 小版本更新，增加Patch版本
        versionData.IncrementVersion();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        ExecuteBuildFlow(versionData.CurrentVersion, BuildType.Hotfix);
    }
    
    private static void ExecuteBuildFlow(VersionNumber version, BuildType buildType)
    { 
        Debug.Log($"[BuildProjectManager] 开始构建热更包 Version: {version}");
        
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        
        // 1. 强制配置 Addressable Settings
        ConfigureAddressableSettings(settings);
        
        // 2. 生成AddressablePackagesEntries，打包进指定Group(HelperBuildData)
        HelperBuildDataExporter.ExportEntries();
        HelperBuildDataExporter.EnsureConfigInGroup();
        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh();
        
        // 3. 构建前清理ServerData
        BuildPathCustomizer.CleanServerData();
        
        // 4. 构建Remote包
        Debug.Log("[BuildProjectManager] 开始执行 Addressables BuildPlayerContent...");
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
        
        if (!string.IsNullOrEmpty(result.Error))
        {
            Debug.LogError($"[BuildProjectManager] 构建失败: {result.Error}");
            return;
        }
        
        // 5. BuildPathCustomizer 整理Remote包目录, 删除不必要的文件
        // 获取 Addressables 默认的 RemoteBuildPath (通常在 ServerData/[Platform])
        string serverDataPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ServerData", EditorUserBuildSettings.activeBuildTarget.ToString());
        string currentVerPackageName = ProjectName + "_" + version.GetVersionString();
        string hotfixOutputDir = Path.Combine(OutputRoot, currentVerPackageName);
        
        BuildPathCustomizer.OrganizeBuildOutput(serverDataPath, hotfixOutputDir);
        
        // 6. 生成 version_state.json 到指定目录
        GenerateVersionStateFile(hotfixOutputDir, version);
        
        // 7. 如果是整包构建，需要导出BuildIndex
        if (buildType == BuildType.Full)
        {
            LocalStatusExporter.ExportBuildIndex();
            LocalStatusExporter.EnsureBuildIndexInGroup();
        }
        
        Debug.Log($"[BuildProjectManager] 热更包构建完毕: {hotfixOutputDir}");
        EditorUtility.RevealInFinder(hotfixOutputDir);
    }

    /// <summary>
    /// 强制配置 Addressable Settings (PackTogetherByLabel, RemotePath 等)
    /// </summary>
    private static void ConfigureAddressableSettings(AddressableAssetSettings settings)
    {
        Debug.Log("[BuildProjectManager] 正在配置 Addressable Settings...");
        
        // 1. 设置 Build Remote Catalog
        settings.BuildRemoteCatalog = true;
        settings.OverridePlayerVersion = "addressables_content_state"; // 保持 Content State 一致，防止 Hash 剧烈变化

        // 2. 遍历 Group 强制设置 BundleMode
        foreach (var group in settings.groups)
        {
            // 跳过部分 Group
            if (group == null) continue;
            
            if (group.Name == "Built In Data" || group.HasSchema<PlayerDataGroupSchema>())
            {
                if (group.HasSchema<BundledAssetGroupSchema>())
                {
                    Debug.LogWarning($"[BuildProjectManager] 修复冲突：移除 {group.Name} 中错误的 BundledAssetGroupSchema");
                    group.RemoveSchema<BundledAssetGroupSchema>();
                    EditorUtility.SetDirty(group);
                }
                continue; 
            }
            
            if (group.Name == HelperBuildDataExporter.GROUP_NAME) continue; // HelperBuildData Group 暂定Together

            var schema = group.GetSchema<BundledAssetGroupSchema>();
            if (schema == null)
            {
                schema = group.AddSchema<BundledAssetGroupSchema>();
            }

            // 统一采用 PackTogetherByLabel （所有包按标签打包）
            if (schema.BundleMode != BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel)
            {
                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogetherByLabel;
                EditorUtility.SetDirty(group);
            }
            
            string currentBuildPathName = schema.BuildPath.GetName(settings);
            
            // 1. HelperBuildData (必要的辅助数据)，必须强制为 Remote，否则无法热更配置
            if (group.Name == HelperBuildDataExporter.GROUP_NAME)
            {
                SetSchemaPathToRemote(settings, schema);
                continue;
            }

            // 2. Local 组保持默认Local路径
            if (currentBuildPathName == AddressableAssetSettings.kLocalBuildPath)
            {
                Debug.Log($"[BuildProjectManager] 保留本地组配置: {group.Name} (LocalBuildPath)");
                continue; 
            }

            // 3. 对Remote组设置 Remote 路径
            // (确保非本地组都被构建到 ServerData 目录下，方便 BuildPathCustomizer 整理)
            SetSchemaPathToRemote(settings, schema);
        }
        
        AssetDatabase.SaveAssets();
    }
    
    /// <summary>
    /// 辅助方法：将 Schema 设置为 Remote 路径
    /// </summary>
    private static void SetSchemaPathToRemote(AddressableAssetSettings settings, BundledAssetGroupSchema schema)
    {
        bool changed = false;
        
        // 检查并设置 BuildPath -> RemoteBuildPath
        if (schema.BuildPath.GetName(settings) != AddressableAssetSettings.kRemoteBuildPath)
        {
            schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteBuildPath);
            changed = true;
        }

        // 检查并设置 LoadPath -> RemoteLoadPath
        if (schema.LoadPath.GetName(settings) != AddressableAssetSettings.kRemoteLoadPath)
        {
            schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kRemoteLoadPath);
            changed = true;
        }

        if (changed)
        {
            Debug.Log($"[BuildProjectManager] 已将 Schema 路径修正为 Remote: {schema.Group.Name}");
        }
    }

    /// <summary>
    /// 生成 version_state.json
    /// </summary>
    private static void GenerateVersionStateFile(string outputDir, VersionNumber version)
    {
        Debug.Log("[BuildProjectManager] 正在生成 version_state.json...");
        
        var versionState = new VersionState
        {
            version = version,
            bundles = new List<BundleInfo>()
        };
        
        // 1. 扫描 bundles 目录下的所有文件
        string bundlesDir = Path.Combine(outputDir, "bundles");
        if (Directory.Exists(bundlesDir))
        {
            var files = Directory.GetFiles(bundlesDir, "*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                
                // 跳过非 bundle 文件（如果有）
                if(file.EndsWith(".manifest")) continue; 

                var bundleInfo = new BundleInfo
                {
                    bundleName = Path.GetFileName(file),
                    hash = HashGenerator.GenerateFileHash(file),
                    size = fileInfo.Length
                };
                versionState.bundles.Add(bundleInfo);
                versionState.totalSize += bundleInfo.size;
            }
        }
        
        // 2. 包体大小预警
        if (versionState.totalSize >= MaxHotfixSizeBytes)
        {
            Debug.LogError($"[BuildProjectManager] 热更包大小过大，需缩减大小: {versionState.totalSize} >= {MaxHotfixSizeBytes}");
            EditorUtility.DisplayDialog("热更包过大", $"热更包大小 ({versionState.totalSize / (1024 * 1024)} MB) 已超过阈值 ({MaxHotfixSizeBytes / (1024 * 1024)} MB)。请缩减资源大小。", "OK");
            return;
        }

        // 3. 计算整个包的 Hash
        // GeneratePackageHash 会遍历目录下所有文件（除了 version_state.json）
        versionState.hash = HashGenerator.GeneratePackageHash(outputDir);

        // 4. 序列化并写入
        string json = JsonUtility.ToJson(versionState, true);
        string savePath = Path.Combine(outputDir, "version_state.json");
        File.WriteAllText(savePath, json);
        
        Debug.Log($"[BuildProjectManager] version_state.json 生成完毕。Hash: {versionState.hash} BundleSize: {versionState.totalSize}");
    }
    
    private static VersionDataBase LoadVersionDataBase()
    {
        VersionDataBase versionData = AssetDatabase.LoadAssetAtPath<VersionDataBase>(versionDataBasePath);
        if (versionData == null)
        {
            Debug.LogError($"[BuildProjectManager] 未找到版本数据库: {versionDataBasePath}");
            return null;
        }
        return versionData;
    }
}
#endif
