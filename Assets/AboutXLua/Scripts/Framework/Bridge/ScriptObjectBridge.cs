using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

public class ScriptObjectBridge : MonoBehaviour,IBridge
{
    [Tooltip("SO配置的Addressable Key")]
    public string configKey;
    
    private ScriptObjectBridgeConfig _config; 
    
    public ScriptableObject GetSO(string key)
    {
        if (_config == null)
        {
            Debug.LogError("[ScriptObjectBridge] 缺少SO配置!");
            return null;
        }
        return _config.GetSO(key);
    }

    public async Task InitializeAsync(LuaTable luaInstance)
    {
        if (string.IsNullOrEmpty(configKey))
        {
            Debug.LogWarning($"[ScriptObjectBridge] {gameObject.name} 未配置 Config Key");
            return;
        }

        _config = await AAPackageManager.Instance.LoadAssetAsync<ScriptObjectBridgeConfig>(configKey);

        if (_config == null)
        {
            Debug.LogError($"[ScriptObjectBridge] 加载配置失败: {configKey}");
        }
    }
    
    // TODO: 可根据需要，根据生命周期进行释放
}

