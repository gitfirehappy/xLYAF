using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;


public class LuaBehaviourBridge : MonoBehaviour
{
    public enum LuaScriptMode
    {
        Class,  // 需要 New() 函数
        Module  // 脚本本身就是实例
    }
    
    public string luaScriptName;
    public TextAsset luaScript;
    public LuaScriptMode luaScriptMode = LuaScriptMode.Class;
    
    private LuaTable luaInstance;
    private LuaFunction onEnableFunc, onDisableFunc;
    private LuaFunction startFunc, updateFunc, fixedUpdateFunc, lateUpdateFunc;
    private LuaFunction onDestroyFunc;

    private List<IBridge> bridges = new List<IBridge>();
    private bool isInitialized = false;
    private bool areBridgesReady = false;
    
    async void Awake()
    {
        await LaunchSignal.WaitForLaunch();
        
        try
        {
            await InitializeLuaInstance();
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LuaBehaviourBridge] 初始化失败: {e.Message}");
            enabled = false; // 初始化失败则禁用组件
        }
    }

    private async Task InitializeLuaInstance()
    {
        var env = LuaEnvManager.Get();
        string nameToLoad = !string.IsNullOrEmpty(luaScriptName) ? luaScriptName : luaScript?.name;
        
        if (string.IsNullOrEmpty(nameToLoad))
        {
            Debug.LogError("[LuaBehaviourBridge] 没有lua脚本名称!");
            return;
        }
        
        var script = env.DoString($"return require('{nameToLoad}')")[0] as LuaTable;

        if (luaScriptMode == LuaScriptMode.Class)
        {
            var newFunc = script.Get<LuaFunction>("New");
            if (newFunc != null)
            {
                // 1. 类模式 (Class Pattern): Lua 脚本有 "New" 函数
                Debug.Log($"[LuaBehaviourBridge] 作为Class初始化 '{nameToLoad}'");
                luaInstance = newFunc.Call(this.gameObject)[0] as LuaTable;
                newFunc.Dispose();
            }
        }
        else
        {
            // 2. 模块模式 (Module Pattern): Lua 脚本没有 "New" 函数，luaInstance 就是脚本本身
            Debug.Log($"[LuaBehaviourBridge] 作为Module初始化 '{nameToLoad}'");
            luaInstance = script;
            // 为模块注入 GameObject 引用，以便在 Lua 中（如果需要）访问
            luaInstance.Set("gameObject", this.gameObject);
            luaInstance.Set("transform", this.transform);
        }

        // 获取所有生命周期函数
        onEnableFunc = luaInstance.Get<LuaFunction>("OnEnable");
        onDisableFunc = luaInstance.Get<LuaFunction>("OnDisable");
        updateFunc = luaInstance.Get<LuaFunction>("Update");
        startFunc = luaInstance.Get<LuaFunction>("Start");
        fixedUpdateFunc = luaInstance.Get<LuaFunction>("FixedUpdate");
        lateUpdateFunc = luaInstance.Get<LuaFunction>("LateUpdate");
        onDestroyFunc = luaInstance.Get<LuaFunction>("OnDestroy");
        
        // 初始化桥接组件
        await InitializeBridges();
        areBridgesReady = true;
        
        // 手动触发 Lua Awake
        var awakeFunc = luaInstance.Get<LuaFunction>("Awake");
        awakeFunc?.Call(luaInstance);
        
        // 手动触发 Lua Start
        startFunc?.Call(luaInstance);
    }

    void Start()
    {
        if (areBridgesReady)
        {
            startFunc?.Call(luaInstance);
        }
    }
    
    void OnEnable()
    {
        if (areBridgesReady)
        {
            onEnableFunc?.Call(luaInstance);
        }
    }

    void OnDisable()
    {
        if (areBridgesReady)
        {
            onDisableFunc?.Call(luaInstance);
        }
    }

    void Update()
    {
        if (isInitialized && areBridgesReady)
        {
            updateFunc?.Call(luaInstance, Time.deltaTime);
        }
    }
    
    void FixedUpdate()
    {
        if (isInitialized && areBridgesReady)
        {
            fixedUpdateFunc?.Call(luaInstance, Time.fixedDeltaTime);
        }
    }
    
    void LateUpdate()
    {
        if (isInitialized && areBridgesReady)
        {
            lateUpdateFunc?.Call(luaInstance);
        }
    }
    
    private async Task InitializeBridges()
    {
        // 获取当前GameObject上所有桥接组件
        var bridgeComponents = GetComponents<IBridge>();
        foreach (var bridge in bridgeComponents)
        {
            await bridge.InitializeAsync(luaInstance);
            bridges.Add(bridge);
            Debug.Log($"[LuaBehaviourBridge] Bridge层初始化: {bridge.GetType().Name}");
        }
    }
    
    void OnDestroy()
    {
        if (areBridgesReady)
        {
            onDestroyFunc?.Call(luaInstance);
        }
        
        // 清理Lua引用
        onEnableFunc?.Dispose();
        onDisableFunc?.Dispose();
        updateFunc?.Dispose();
        startFunc?.Dispose();
        fixedUpdateFunc?.Dispose();
        lateUpdateFunc?.Dispose();
        onDestroyFunc?.Dispose();
        
        bridges.Clear();
        luaInstance?.Dispose();
    }
}
