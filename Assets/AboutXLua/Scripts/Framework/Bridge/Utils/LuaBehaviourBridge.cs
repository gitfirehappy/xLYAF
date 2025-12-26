using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

/// <summary>
/// Lua 桥接器。
/// 负责加载一个或多个 Lua 脚本 (通过 SO)，并将其生命周期(Update, OnEnable等)与 Unity 同步。
/// 同时负责初始化此 GameObject 上的所有 IBridge 组件，并将 Lua 实例注入其中。
/// </summary>
public class LuaBehaviourBridge : MonoBehaviour
{
    /// <summary>
    /// 定义 Lua 脚本的加载模式
    /// </summary>
    public enum LuaScriptMode
    {
        Class,  // 需要 New() 函数
        Module  // 脚本本身就是实例
    }
    
    [Header("Lua 脚本配置")]
    [Tooltip("填写 LuaBehaviourConfigSO 的 Addressable Key")]
    public string configKey; 

    private LuaBehaviourConfigSO _multiScriptConfig;

    /// <summary>
    /// 内部类，用于保存每个 Lua 脚本的运行时实例和缓存的函数
    /// </summary>
    private class LuaRuntimeInstance
    {
        public LuaTable instance;
        public string scriptName;
        
        public LuaFunction onEnableFunc, onDisableFunc;
        public LuaFunction startFunc, updateFunc, fixedUpdateFunc, lateUpdateFunc;
        public LuaFunction onDestroyFunc;
        public LuaFunction awakeFunc;

        public LuaRuntimeInstance(LuaTable inst, string name)
        {
            instance = inst;
            scriptName = name;
        }

        public void CacheFunctions()
        {
            if (instance == null) return;
            awakeFunc = instance.Get<LuaFunction>("Awake");
            startFunc = instance.Get<LuaFunction>("Start");
            onEnableFunc = instance.Get<LuaFunction>("OnEnable");
            onDisableFunc = instance.Get<LuaFunction>("OnDisable");
            updateFunc = instance.Get<LuaFunction>("Update");
            fixedUpdateFunc = instance.Get<LuaFunction>("FixedUpdate");
            lateUpdateFunc = instance.Get<LuaFunction>("LateUpdate");
            onDestroyFunc = instance.Get<LuaFunction>("OnDestroy");
        }

        public void Dispose()
        {
            awakeFunc?.Dispose();
            startFunc?.Dispose();
            onEnableFunc?.Dispose();
            onDisableFunc?.Dispose();
            updateFunc?.Dispose();
            fixedUpdateFunc?.Dispose();
            lateUpdateFunc?.Dispose();
            onDestroyFunc?.Dispose();
            instance?.Dispose();
            
            // 清空引用
            awakeFunc = startFunc = onEnableFunc = onDisableFunc = updateFunc = fixedUpdateFunc = lateUpdateFunc = onDestroyFunc = null;
            instance = null;
        }
    }

    private List<LuaRuntimeInstance> luaInstances = new List<LuaRuntimeInstance>();
    private List<IBridge> bridges = new List<IBridge>();
    private bool isInitialized = false;
    private bool areBridgesReady = false;
    
    async void Awake()
    {
        // 确保在主逻辑执行前等待
        await LaunchSignal.WaitForLaunch();
        
        try
        {
            await InitializeAllLuaInstances();
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LuaBehaviourBridge] 在 {gameObject.name} 上初始化失败: {e.Message}\n{e.StackTrace}", gameObject);
            enabled = false; // 初始化失败则禁用组件
        }
    }

    private async Task InitializeAllLuaInstances()
    {
        var env = LuaEnvManager.Get();
        
        if (string.IsNullOrEmpty(configKey))
        {
            Debug.LogError($"[LuaBehaviourBridge] {gameObject.name} 未配置 Config Key", gameObject);
            return;
        }

        _multiScriptConfig = await AAPackageManager.Instance.LoadAssetAsync<LuaBehaviourConfigSO>(configKey);
        
        // 1. 检查 SO 配置
        if (_multiScriptConfig == null || _multiScriptConfig.scriptsToLoad.Count == 0)
        {
            Debug.LogError($"[LuaBehaviourBridge] 在 {gameObject.name} 上没有配置任何 Lua 脚本 (multiScriptConfig 为空或列表为空)。", gameObject);
            return;
        }

        // 2. 获取所有桥接组件
        // GetComponents<IBridge>() 会找到此 GameObject 上的所有实现了 IBridge 接口的 C# 组件
        var bridgeComponents = GetComponents<IBridge>();
        foreach (var bridge in bridgeComponents)
        {
            bridges.Add(bridge);
        }

        // 3. 遍历配置，创建和初始化每个 Lua 实例
        foreach (var config in _multiScriptConfig.scriptsToLoad)
        {
            string nameToLoad = config.GetScriptLoadName();
            if (string.IsNullOrEmpty(nameToLoad))
            {
                Debug.LogWarning($"[LuaBehaviourBridge] 跳过一个空的脚本配置。");
                continue;
            }

            LuaTable newLuaInstance = null;
            try
            {
                // Require 脚本
                var script = env.DoString($"return require('{nameToLoad}')")[0] as LuaTable;
                if (script == null)
                {
                    Debug.LogError($"[LuaBehaviourBridge] Require 脚本 '{nameToLoad}' 失败，返回值不是 LuaTable。", gameObject);
                    continue;
                }

                // 根据模式创建实例
                if (config.luaScriptMode == LuaScriptMode.Class)
                {
                    var newFunc = script.Get<LuaFunction>("New");
                    if (newFunc != null)
                    {
                        Debug.Log($"[LuaBehaviourBridge] 作为 Class 初始化 '{nameToLoad}'");
                        newLuaInstance = newFunc.Call(this.gameObject)[0] as LuaTable;
                        newFunc.Dispose();
                    }
                    else
                    {
                        Debug.LogError($"[LuaBehaviourBridge] 脚本 '{nameToLoad}' 配置为 Class 模式，但未找到 New() 函数。", gameObject);
                        continue;
                    }
                }
                else // Module 模式
                {
                    Debug.Log($"[LuaBehaviourBridge] 作为 Module 初始化 '{nameToLoad}'");
                    newLuaInstance = script;
                    newLuaInstance.Set("gameObject", this.gameObject);
                    newLuaInstance.Set("transform", this.transform);
                }

                if (newLuaInstance == null)
                {
                    Debug.LogError($"[LuaBehaviourBridge] 无法创建 '{nameToLoad}' 的 Lua 实例。", gameObject);
                    continue;
                }
                
                // 创建运行时包装器并缓存函数
                var runtimeInstance = new LuaRuntimeInstance(newLuaInstance, nameToLoad);
                runtimeInstance.CacheFunctions();

                // 为这个新创建的 Lua 实例初始化所有桥接组件
                foreach (var bridge in bridges)
                {
                    await bridge.InitializeAsync(runtimeInstance.instance);
                    Debug.Log($"[LuaBehaviourBridge] Bridge '{bridge.GetType().Name}' 已为 Lua 实例 '{nameToLoad}' 初始化。");
                }
                
                // 手动触发 Lua Awake (在 Start 之前)
                runtimeInstance.awakeFunc?.Call(runtimeInstance.instance);
                
                // 添加到管理列表
                luaInstances.Add(runtimeInstance);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LuaBehaviourBridge] 初始化脚本 '{nameToLoad}' 失败: {e.Message}\n{e.StackTrace}", gameObject);
                // 确保释放失败的实例（如果已创建）
                newLuaInstance?.Dispose();
            }
        }
        
        areBridgesReady = true;
        
        // 4. 手动触发所有 Lua Start
        foreach (var lua in luaInstances)
        {
            lua.startFunc?.Call(lua.instance);
        }
    }

    #region Unity生命周期桥接

    void Start()
    {
        if (areBridgesReady)
        {
            foreach (var lua in luaInstances)
            {
                lua.startFunc?.Call(lua.instance);
            }
        }
    }
    
    void OnEnable()
    {
        if (areBridgesReady)
        {
            foreach (var lua in luaInstances)
            {
                lua.onEnableFunc?.Call(lua.instance);
            }
        }
    }

    void OnDisable()
    {
        if (areBridgesReady)
        {
            foreach (var lua in luaInstances)
            {
                lua.onDisableFunc?.Call(lua.instance);
            }
        }
    }

    void Update()
    {
        if (isInitialized && areBridgesReady)
        {
            float dt = Time.deltaTime;
            foreach (var lua in luaInstances)
            {
                lua.updateFunc?.Call(lua.instance, dt);
            }
        }
    }
    
    void FixedUpdate()
    {
        if (isInitialized && areBridgesReady)
        {
            float fdt = Time.fixedDeltaTime;
            foreach (var lua in luaInstances)
            {
                lua.fixedUpdateFunc?.Call(lua.instance, fdt);
            }
        }
    }
    
    void LateUpdate()
    {
        if (isInitialized && areBridgesReady)
        {
            foreach (var lua in luaInstances)
            {
                lua.lateUpdateFunc?.Call(lua.instance);
            }
        }
    }
    
    void OnDestroy()
    {
        // 1. 调用 Lua 的 OnDestroy
        if (areBridgesReady)
        {
            foreach (var lua in luaInstances)
            {
                try
                {
                    lua.onDestroyFunc?.Call(lua.instance);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LuaBehaviourBridge] 在 OnDestroy 期间调用脚本 '{lua.scriptName}' 失败: {e.Message}", gameObject);
                }
            }
        }
        
        // 2. 清理所有 Lua 引用
        foreach (var lua in luaInstances)
        {
            lua.Dispose();
        }
        luaInstances.Clear();
        
        bridges.Clear();
    }
    
    #endregion
}