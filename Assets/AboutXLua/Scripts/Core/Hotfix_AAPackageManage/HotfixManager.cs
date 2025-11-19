using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class HotfixManager
{
    public async static Task InitializeAsync()
    {
        // 1. 初始化 Addressable 本地包
        // 初始化 Addressables 系统，加载本地 Catalog
        AsyncOperationHandle<IResourceLocator> initHandle = Addressables.InitializeAsync();
        await initHandle.Task;

        if (initHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[HotfixManager] Addressables 初始化失败: {initHandle.OperationException}");
            return;
        }
        Debug.Log("[HotfixManager] 1. Addressables 本地包初始化成功");
        
        // 2. 下载远端更新包（HelperBuildData，catalog，bundles）
        
        // 3. 版本比对
        
        // 若旧版本包有更新
        if (1 != 0)
        {
            // 4. 合并catalog（Remote > Local）
            
            // 5. 删除旧版本包
        }
        
        // 6. AAPackageManager 基于本地索引扫描构建 （依赖更新后的HelperBuildData）
        
        // 此时正式开放AAPackageManager 的获取资源功能API
        AAPackageManager.Instance.SetHasUpdateCatalog();
    }
}
