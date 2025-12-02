using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class HotfixManager
{
    private static readonly string _hotfixUrl = "https://your-site-name.netlify.app/HotfixOutput";
    
    // 固定下载 manifest 动态获取路径
    private static readonly string _manifestUrl = Path.Combine(_hotfixUrl, "manifest.json");
    
    private static string _remoteUrlRoot;
    
    public async static Task InitializeAsync()
    {
        // 1. 初始化 Addressable 本地包
        // 初始化 Addressables 系统，加载本地 Catalog
        var initHandle = Addressables.InitializeAsync();
        await initHandle.Task;
        
        if (initHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[HotfixManager] Addressables 初始化失败: {initHandle.OperationException}");
            return;
        }
        Debug.Log("[HotfixManager] Addressables 本地包初始化成功");
        
        // 2. 加载 BuildIndex，并初始化路径 (从 Local AA 包中)
        var indexHandle = Addressables.LoadAssetAsync<BuildIndex>(BuildIndex.ASSET_ADDRESS);
        BuildIndex buildIndex = await indexHandle.Task;

        if (indexHandle.Status != AsyncOperationStatus.Succeeded || buildIndex == null)
        {
            Debug.LogError("[HotfixManager] 致命错误：无法加载 BuildIndex！无法确定版本路径。");
            return;
        }
        
        PathManager.Initialize(buildIndex);
        PathManager.EnsureDirectories();
        
        // 3. 获取 manifest.json，确定下载路径
        string manifestJson = await NetworkDownloader.Instance.DownloadText(_manifestUrl);
        if (string.IsNullOrEmpty(manifestJson))
        {
            Debug.LogError("[HotfixManager] 无法获取manifest.json，使用本地资源运行。");
            await FinishHotfix();
            return;
        }
        
        Manifest manifest = JsonUtility.FromJson<Manifest>(manifestJson);
        if (string.IsNullOrEmpty(manifest.latestPackage))
        {
            Debug.LogError("[HotfixManager] manifest.json 无效，使用本地资源运行。");
            await FinishHotfix();
            return;
        }
        
        string packagePath = manifest.latestPackage;
        _remoteUrlRoot = Path.Combine(_hotfixUrl,"Packages", packagePath,"/");
    
        Debug.Log($"[HotfixManager] 获取最新包体: {packagePath}，URL已更新: {_remoteUrlRoot}");

        
        // 4. 加载本地 version_state.json
        VersionChecker versionChecker = new VersionChecker();
        string localVersionStatePath = Path.Combine(PathManager.LocalRoot, "version_state.json");
        VersionState localVersionState = null;
        
        if (File.Exists(localVersionStatePath))
        {
            localVersionState = versionChecker.ParseJson(File.ReadAllText(localVersionStatePath));
            Debug.Log($"[HotfixManager] 本地版本: {localVersionState?.version}, Hash: {localVersionState?.hash}");
        }
        
        // 5. 下载远端 version_state.json
        string remoteVersionUrl = _remoteUrlRoot + "version_state.json";
        string remoteVersionJson = await NetworkDownloader.Instance.DownloadText(remoteVersionUrl);
        
        if (string.IsNullOrEmpty(remoteVersionJson))
        {
            Debug.LogError("[HotfixManager] 无法获取远端版本信息，将使用本地资源运行。");
            await FinishHotfix();
            return;
        }
        
        VersionState remoteVersionState = versionChecker.ParseJson(remoteVersionJson);
        
        // 如果是大版本更新，则强制清理所有热更目录
        if (localVersionState != null)
        {
            if (versionChecker.IsMajorUpdate(localVersionState.version, remoteVersionState.version))
            {
                Debug.LogWarning($"[HotfixManager] 检测到大版本更新 (Local:{localVersionState.version} -> Remote:{remoteVersionState.version})。执行全量清理。");
                
                // 强制清理所有热更目录 (Local, Remote, Temp)
                PackageCleaner.Instance.ClearAllHotfix();
                
                // 重置本地状态对象为 null，迫使后续逻辑进行全量下载
                localVersionState = null; 
            }
        }
        
        // 6. 比较版本差异
        VersionDiffResult diff = versionChecker.CalculateDiff(localVersionState, remoteVersionState);
        
        if (!diff.HasUpdate)
        {
            Debug.Log("[HotfixManager] 本地版本已是最新，将使用本地资源运行。");
            await FinishHotfix();
            return;
        }
        
        Debug.Log($"[HotfixManager] 发现更新！需下载Bundle数: {diff.DownloadList.Count}, 总大小: {diff.TotalDownloadSize}");
        
        // 7. 下载差异 bundle 到 RemoteRoot （暂存远端文件）
        // HelperBuildData的bundle组都会在这一步下载
        string remoteBundleRoot = PathManager.RemoteBundleRoot;
        if (!Directory.Exists(remoteBundleRoot)) Directory.CreateDirectory(remoteBundleRoot);
        
        foreach (var bundleInfo in diff.DownloadList)
        {
            string bundleUrl = _remoteUrlRoot + "bundles/" + bundleInfo.bundleName;
            string savePath = Path.Combine(remoteBundleRoot, bundleInfo.bundleName);
            
            // TODO: 这里可以扩展为并行下载或队列下载
            bool success = await NetworkDownloader.Instance.DownloadFile(bundleUrl, savePath);
            if (!success)
            {
                Debug.LogError($"[HotfixManager] 致命错误：Bundle下载失败 {bundleInfo.bundleName}");
                return; // 中断热更，弹出重试 UI
            }
        }
        
        // 8. 下载 catalog.json
        string catalogUrl = _remoteUrlRoot + "catalog.json";
        await NetworkDownloader.Instance.DownloadFile(catalogUrl, Path.Combine(PathManager.RemoteRoot, "catalog.json"));
        
        // 保存新的 version_state.json 到RemoteRoot
        File.WriteAllText(Path.Combine(PathManager.RemoteRoot, "version_state.json"), remoteVersionJson);
        
        Debug.Log("[HotfixManager] 热更资源下载完成，开始应用热更...");
        
        // 9. 应用更新
        PackageCleaner.Instance.ApplyUpdate(diff.DeleteList, PathManager.RemoteRoot, PathManager.LocalRoot);
        
        // 10. 加载新的 catalog
        Debug.Log("[HotfixManager] 加载新 Catalog...");
        string localCatalogPath = Path.Combine(PathManager.LocalRoot, "catalog.json");
        
        CatalogUpdater catalogUpdater = new CatalogUpdater();
        bool catalogLoaded = await catalogUpdater.LoadExternalCatalog(localCatalogPath);

        if (catalogLoaded)
        {
            Debug.Log("[HotfixManager] 热更流程成功完成！");
        }
        
        await FinishHotfix();
    }
    
    public static async Task FinishHotfix()
    {
        // AAPackageManager 基于本地索引扫描构建 （依赖更新后的HelperBuildData）
        await AAPackageManager.Instance.Initialize();
        
        // 此时正式开放AAPackageManager 的获取资源功能API
        AAPackageManager.Instance.SetHasUpdateCatalog();
    }
}
