using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

public class ScriptObjectBridge : MonoBehaviour,IBridge
{
    public ScriptObjectBridgeConfig config;

    public ScriptableObject GetSO(string key)
    {
        if (config == null)
        {
            Debug.LogError("[ScriptObjectBridge] 缺少SO配置!");
            return null;
        }
        return config.GetSO(key);
    }

    public async Task InitializeAsync(LuaTable luaInstance)
    {
        await Task.CompletedTask;
    }
}

