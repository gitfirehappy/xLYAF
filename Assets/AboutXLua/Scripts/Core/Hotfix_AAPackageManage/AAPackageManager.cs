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
    
    private AddressableLabelsConfig _config;

    private bool _isInitialized = false;

    public async Task Initialize()
    {
        // 异步加载配置SO
        AsyncOperationHandle<AddressableLabelsConfig> handle = 
            Addressables.LoadAssetAsync<AddressableLabelsConfig>(CONFIG_ASSET_KEY);
        
        _config = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded || _config == null)
        {
            Debug.LogError($"[AAPackageManager] 关键配置加载失败: {CONFIG_ASSET_KEY}。管理器无法初始化。");
            return;
        }
        
        _isInitialized = true;
        Debug.Log($"[AAPackageManager] 初始化完成。Entries: {_config.allEntries.Count}");
    }

    #region 查询接口：核心

    /// <summary>
    /// 获取某类型的所有 Key
    /// </summary>
    public List<string> GetKeysByType(string type)
    {
        return _isInitialized ? _config.GetKeysByType(type) : new List<string>();
    }

    /// <summary>
    /// 获取某标签的所有 Key
    /// </summary>
    public List<string> GetKeysByLabel(string label)
    {
        return _isInitialized ? _config.GetKeysByLabel(label) : new List<string>();
    }

    /// <summary>
    /// 复合查询：获取同时满足 Type 和 Label 的 Key
    /// </summary>
    public List<string> GetKeysByTypeAndLabel(string type, string label)
    {
        if (!_isInitialized) return new List<string>();
        
        // 获取Type的所有Key
        var typeKeys = _config.GetKeysByType(type);
        // 获取Label的所有Key (利用HashSet优化交集查找)
        var labelKeys = new HashSet<string>(_config.GetKeysByLabel(label));
        
        return typeKeys.Where(k => labelKeys.Contains(k)).ToList();
    }

    /// <summary>
    /// 检查资源是否存在
    /// </summary>
    public bool ContainsKey(string key)
    {
        return _isInitialized && _config.allEntries.Any(e => e.key == key);
    }

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
