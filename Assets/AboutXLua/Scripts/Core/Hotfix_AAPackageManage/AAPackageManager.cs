using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 资源库索引：负责管理资源的元数据（Type, Label），提供查询功能
/// </summary>
public class AAPackageManager : Singleton<AAPackageManager>
{
    // AddressablePackagesEntries SO配置的 Addressable Key，确保一致
    private const string CONFIG_ASSET_KEY = "AddressableLabelsConfig";
    
    // 核心数据：Key -> Entry
    private Dictionary<string, PackageEntry> _entryLookup = new();
    
    // 核心索引：Type -> Keys (只存Key，省内存)
    private Dictionary<string, List<string>> _keysByType = new();
    
    // 核心索引：Label -> Keys
    private Dictionary<string, List<string>> _keysByLabel = new();

    private bool _isInitialized = false;

    public async Task Initialize()
    {
        // 异步加载配置SO
        AsyncOperationHandle<AddressableLabelsConfig> handle = 
            Addressables.LoadAssetAsync<AddressableLabelsConfig>(CONFIG_ASSET_KEY);
        
        var config = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded || config == null)
        {
            Debug.LogError($"[AAPackageManager] 关键配置加载失败: {CONFIG_ASSET_KEY}。管理器无法初始化。");
            return;
        }

        // 构建索引
        BuildAAIndex(config.allEntries);
        
        // 释放句柄（已经拿到了数据）
        Addressables.Release(handle);
        _isInitialized = true;
    }
    
    private void BuildAAIndex(List<PackageEntry> entries)
    {
        _entryLookup.Clear();
        _keysByType.Clear();
        _keysByLabel.Clear();

        foreach (var entry in entries)
        {
            // Lookup
            if (!_entryLookup.TryAdd(entry.key, entry)) continue; // 避免重复

            // By Type
            if (!_keysByType.ContainsKey(entry.Type)) 
                _keysByType[entry.Type] = new List<string>();
            _keysByType[entry.Type].Add(entry.key);

            // By Label
            foreach (var label in entry.Labels)
            {
                if (!_keysByLabel.ContainsKey(label)) 
                    _keysByLabel[label] = new List<string>();
                _keysByLabel[label].Add(entry.key);
            }
        }
        Debug.Log($"[AAPackageManager] 索引构建完成。Entries: {entries.Count}");
    }

    #region 查询接口：核心

    /// <summary>
    /// 获取某类型的所有 Key
    /// </summary>
    public List<string> GetKeysByType(string type)
    {
        return _keysByType.TryGetValue(type, out var list) ? list : new List<string>();
    }

    /// <summary>
    /// 获取某标签的所有 Key
    /// </summary>
    public List<string> GetKeysByLabel(string label)
    {
        return _keysByLabel.TryGetValue(label, out var list) ? list : new List<string>();
    }

    /// <summary>
    /// 复合查询：获取同时满足 Type 和 Label 的 Key
    /// </summary>
    public List<string> GetKeysByTypeAndLabel(string type, string label)
    {
        if (!_keysByType.TryGetValue(type, out var typeKeys)) return new List<string>();
        
        return typeKeys.Where(key => 
        {
            var entry = _entryLookup[key];
            return entry.Labels.Contains(label);
        }).ToList();
    }
    
    /// <summary>
    /// 检查资源是否存在
    /// </summary>
    public bool ContainsKey(string key) => _entryLookup.ContainsKey(key);

    #endregion

    #region 上层加载辅助：可选

    public async Task<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object
    {
        if(!_isInitialized) Debug.LogWarning("AAPackageManager 未初始化");

        return await Addressables.LoadAssetAsync<T>(key).Task;
    }

    public async Task<List<T>> LoadAssetsByTypeAsync<T>(string type) where T : UnityEngine.Object
    {
        var keys = GetKeysByType(type);
        if (keys.Count == 0) return new List<T>();

        // 批量加载 Key
        return (await Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union).Task).ToList();
    }
    
    public async Task<List<T>> LoadAssetsByTypeAndLabelAsync<T>(string type, string label) where T : UnityEngine.Object
    {
        var keys = GetKeysByTypeAndLabel(type, label);
        if (keys.Count == 0) return new List<T>();

        return (await Addressables.LoadAssetsAsync<T>(keys, null, Addressables.MergeMode.Union).Task).ToList();
    }
    #endregion
}
