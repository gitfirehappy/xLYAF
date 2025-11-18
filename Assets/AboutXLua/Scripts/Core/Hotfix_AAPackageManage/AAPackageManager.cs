using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AAPackageManager : Singleton<AAPackageManager>
{
    // AddressablePackagesEntries SO配置的 Addressable Key，确保一致
    private const string CONFIG_ASSET_KEY = "BuiltInData/AddressablePackagesEntries";
    
    private Dictionary<string, PackageEntry> _dataDomain = new();
    private RemoteIndex _remoteIndex;
    private bool _hasUpdateCatalog = false;
    public bool SetHasUpdateCatalog() => _hasUpdateCatalog = true;
    
    // 按类型分类的包索引
    private Dictionary<string, List<PackageEntry>> _packagesByType = new();
    private Dictionary<string, List<PackageEntry>> _packagesByLabel = new();

    public async Task Initialize(RemoteIndex remoteIndex)
    {
        _remoteIndex = remoteIndex;

        // 1. 异步加载配置SO
        AsyncOperationHandle<AddressablePackagesEntries> loadHandle = 
            Addressables.LoadAssetAsync<AddressablePackagesEntries>(CONFIG_ASSET_KEY);
        
        AddressablePackagesEntries localConfig = await loadHandle.Task;

        if (loadHandle.Status != AsyncOperationStatus.Succeeded || localConfig == null)
        {
            Debug.LogError($"[AAPackageManager] 关键配置加载失败: {CONFIG_ASSET_KEY}。管理器无法初始化。");
            return;
        }

        // 2. 扫描并构建索引
        ScanLocalPackages(localConfig);
        
        // 3. 释放句柄（已经拿到了数据）
        Addressables.Release(loadHandle);
    }
    
    /// <summary>
    /// 使用从 SO 加载的数据构建运行时索引
    /// </summary>
    private void ScanLocalPackages(AddressablePackagesEntries localConfig)
    { 
        // 清空现有数据
        _dataDomain.Clear();
        _packagesByType.Clear();
        _packagesByLabel.Clear();
        
        // 遍历从 SO 加载的条目数据
        foreach (var runtimeEntry in localConfig.allEntries)
        {
            // 1. 构建 DataDomain (Key -> Entry)
            if (!_dataDomain.ContainsKey(runtimeEntry.key))
            {
                _dataDomain.Add(runtimeEntry.key, runtimeEntry);
            }
            else
            {
                Debug.LogWarning($"[AAPackageManager] 发现重复的 Key: {runtimeEntry.key}。请检查 Addressables 设置。");
            }

            // 2. 构建 Type 索引 (Type -> List<Entry>)
            if (!_packagesByType.ContainsKey(runtimeEntry.Type))
            {
                _packagesByType[runtimeEntry.Type] = new List<PackageEntry>();
            }
            _packagesByType[runtimeEntry.Type].Add(runtimeEntry);

            // 3. 构建 Label 索引 (Label -> List<Entry>)
            foreach (string label in runtimeEntry.Labels)
            {
                if (!_packagesByLabel.ContainsKey(label))
                {
                    _packagesByLabel[label] = new List<PackageEntry>();
                }
                _packagesByLabel[label].Add(runtimeEntry);
            }
        }
        
        Debug.Log($"[AAPackageManager] 扫描完成: {_dataDomain.Count} 个资源包, {_packagesByType.Count} 个类型, {_packagesByLabel.Count} 个标签");
    }
    
    public PackageEntry GetPackageEntry(string key)
    {
        if (!_dataDomain.ContainsKey(key))
        {
            Debug.LogWarning($"[AAPackageManager] 未找到资源包: {key}");
            return null;
        }
        
        // 路径由 Addressables & merged catalog 决定
        // TODO: 获取前需要 RemoteIndex 检查
        return _dataDomain[key]; // 包信息（key/label/type）
    }

    #region 上层封装接口

    /// <summary>
    /// 根据key 异步加载单个资源
    /// </summary>>
    /// <param name="key">AA包的key</param>
    /// <returns></returns>
    public async Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
    {
        if (!_hasUpdateCatalog)
        {
            Debug.LogError("[AAPackageManager] RemoteCatalog 尚未更新！");
        }

        PackageEntry entry = GetPackageEntry(key);
        if (entry == null)
        {
            return null;
        }

        // 实际加载
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
        T result = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AAPackageManager] 异步加载资源失败: {key} - {handle.OperationException}");
            Addressables.Release(handle); // 失败时释放
            return null;
        }
        
        return result;
    }

    /// <summary>
    /// 异步加载指定标签的资源
    /// </summary>
    /// <param name="label">AA包标签</param>
    /// <returns></returns>
    public async Task<List<T>> LoadAssetByLabel<T>(string label) where T : UnityEngine.Object
    {
        if (!_hasUpdateCatalog)
        {
            Debug.LogError("[AAPackageManager] RemoteCatalog 尚未更新！");
        }
        
        if (!_packagesByLabel.ContainsKey(label))
        {
            Debug.LogWarning($"[AAPackageManager] 未找到标签: {label}");
            return new List<T>();
        }
        
        var entries = _packagesByLabel[label];
        if (entries.Count == 0)
        {
            return new List<T>();
        }

        // 提取所有 keys
        var keys = entries.Select(entry => entry.key);

        // 异步批量加载
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union);
        
        IList<T> results = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AAPackageManager] 异步加载标签资源失败: {label} - {handle.OperationException}");
            Addressables.Release(handle);
            return new List<T>();
        }
        
        return new List<T>(results);
    } 
    
    /// <summary>
    /// 异步加载指定类型的资源
    /// </summary>
    /// <param name="type">要加载的类型</param>
    /// <returns></returns>
    public async Task<List<T>> LoadAssetByType<T>(string type) where T : UnityEngine.Object
    {
        if (!_hasUpdateCatalog)
        {
            Debug.LogError("[AAPackageManager] RemoteCatalog 尚未更新！");
        }
        
        if (!_packagesByType.ContainsKey(type))
        {
            Debug.LogWarning($"[AAPackageManager] 未找到类型: {type}");
            return new List<T>();
        }

        var entries = _packagesByType[type];
        if (entries.Count == 0)
        {
            return new List<T>();
        }

        // 提取所有 keys
        var keys = entries.Select(entry => entry.key);

        // 异步批量加载
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union);
        
        IList<T> results = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AAPackageManager] 异步加载类型资源失败: {type} - {handle.OperationException}");
            Addressables.Release(handle);
            return new List<T>();
        }

        return new List<T>(results);
    }
    
    /// <summary>
    /// 异步加载指定类型和标签的资源
    /// </summary>
    /// <param name="type">要加载的类型</param>
    /// <param name="label">标签</param>
    /// <returns></returns>
    public async Task<List<T>> LoadAssetByTypeAndLabel<T>(string type, string label) where T : UnityEngine.Object
    {
        if (!_hasUpdateCatalog)
        {
            Debug.LogError("[AAPackageManager] RemoteCatalog 尚未更新！");
        }
        
        if (!_packagesByType.ContainsKey(type))
        {
            Debug.LogWarning($"[AAPackageManager] 未找到类型: {type}。无法执行 Type & Label 加载。");
            return new List<T>();
        }
        
        // 1. 获取该类型的所有条目
        var typeEntries = _packagesByType[type];

        // 2. 筛选出同时包含指定 label 的条目
        var matchingKeys = typeEntries
            .Where(entry => entry.Labels.Contains(label))
            .Select(entry => entry.key)
            .ToList(); // 转换为 List

        if (matchingKeys.Count == 0)
        {
            Debug.Log($"[AAPackageManager] 未找到 Type={type} 且 Label={label} 的资源。");
            return new List<T>();
        }

        // 3. 异步批量加载
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(matchingKeys, null, Addressables.MergeMode.Union);
        
        IList<T> results = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[AAPackageManager] 异步加载 Type & Label 资源失败: {type} & {label} - {handle.OperationException}");
            Addressables.Release(handle);
            return new List<T>();
        }

        return new List<T>(results);
    }

    #endregion
}
