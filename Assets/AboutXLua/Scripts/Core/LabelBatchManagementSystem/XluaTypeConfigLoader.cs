// File: Assets/Scripts/XLuaConfig/XluaTypeConfigLoader.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class XluaTypeConfigLoader
{
    public static List<Type> HotfixTypes { get; private set; }
    public static List<Type> LuaCallCSharpTypes { get; private set; }
    public static List<Type> CSharpCallLuaTypes { get; private set; }

    // 地址标签，用于在Addressables中识别TypeListSO资产
    private const string XLUA_CONFIG_LABEL = "XLuaConfigs"; 

    // 初始化方法，会在底层启动时调用
    public static void Init()
    {
        Debug.Log("XluaTypeConfigLoader: Initializing type lists...");

        // 1. 初始化列表
        HotfixTypes = new List<Type>();
        LuaCallCSharpTypes = new List<Type>();
        CSharpCallLuaTypes = new List<Type>();

        // 2. 使用Addressables加载所有 TypeListSO
        // 注意：这里使用 WaitForCompletion() 是为了在游戏启动初期进行同步加载
        // 如果您的启动流程是异步的，或者Addressables初始化在更早阶段完成，
        // 可以将其改为异步加载并等待直到完成。
        AsyncOperationHandle<IList<TypeListSO>> loadHandle = Addressables.LoadAssetsAsync<TypeListSO>(XLUA_CONFIG_LABEL, null);
        IList<TypeListSO> allConfigs;

        try
        {
            allConfigs = loadHandle.WaitForCompletion(); // 同步等待加载完成
        }
        catch (Exception ex)
        {
            Debug.LogError($"XluaTypeConfigLoader: Failed to load XLua configs from Addressables label '{XLUA_CONFIG_LABEL}'. Error: {ex.Message}");
            return; // 加载失败，直接返回
        }
        finally
        {
            Addressables.Release(loadHandle); // 释放加载句柄
        }

        if (allConfigs == null || allConfigs.Count == 0)
        {
            Debug.LogWarning($"XluaTypeConfigLoader: No TypeListSO assets found with label '{XLUA_CONFIG_LABEL}'. Please ensure they are addressable and tagged correctly.");
            return;
        }

        // 3. 遍历所有配置，并根据Tag分类
        foreach (var config in allConfigs)
        {
            if (config == null)
            {
                Debug.LogWarning("XluaTypeConfigLoader: Found a null TypeListSO asset during loading.");
                continue;
            }

            // 将 TypeReference 解析为 Type
            // SelectMany 用于将多个列表合并成一个扁平的列表
            var resolvedTypes = config.types
                                      .Select(tr => tr.GetTypeCache())
                                      .Where(t => t != null) // 过滤掉无法解析的类型
                                      .ToList();

            if (resolvedTypes.Count == 0)
            {
                Debug.LogWarning($"XluaTypeConfigLoader: TypeListSO '{config.name}' (Tag: {config.tag}) contains no valid types after resolution.");
                continue;
            }

            switch (config.tag)
            {
                case TypeListSO.ConfigTag.Hotfix:
                    HotfixTypes.AddRange(resolvedTypes);
                    Debug.Log($"XluaTypeConfigLoader: Loaded {resolvedTypes.Count} Hotfix types from '{config.name}'.");
                    break;
                case TypeListSO.ConfigTag.LuaCallCSharp:
                    LuaCallCSharpTypes.AddRange(resolvedTypes);
                    Debug.Log($"XluaTypeConfigLoader: Loaded {resolvedTypes.Count} LuaCallCSharp types from '{config.name}'.");
                    break;
                case TypeListSO.ConfigTag.CSharpCallLua:
                    CSharpCallLuaTypes.AddRange(resolvedTypes);
                    Debug.Log($"XluaTypeConfigLoader: Loaded {resolvedTypes.Count} CSharpCallLua types from '{config.name}'.");
                    break;
                default:
                    Debug.LogWarning($"XluaTypeConfigLoader: Unknown ConfigTag '{config.tag}' in TypeListSO '{config.name}'.");
                    break;
            }
        }

        Debug.Log($"XluaTypeConfigLoader: Initialization complete. Hotfix: {HotfixTypes.Count}, LuaCallCSharp: {LuaCallCSharpTypes.Count}, CSharpCallLua: {CSharpCallLuaTypes.Count} types loaded.");
    }
    

    // 后续在您的XLua初始化代码中，可以这样使用:
    // private static List<Type> LuaCallCSharp = XluaTypeConfigLoader.LuaCallCSharpTypes;
    // private static List<Type> CSharpCallLua = XluaTypeConfigLoader.CSharpCallLuaTypes;
    // private static List<Type> Hotfix = XluaTypeConfigLoader.HotfixTypes;
}