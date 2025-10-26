using System.Collections.Generic;
using UnityEngine;
using XLua;


public class LuaBehaviour : MonoBehaviour
{
    public string luaScriptName;
    private LuaTable luaInstance;
    private LuaFunction onEnableFunc, onDisableFunc;
    private LuaFunction startFunc, updateFunc, fixedUpdateFunc, lateUpdateFunc;
    private LuaFunction onDestroyFunc;

    private List<IBridge> bridges = new List<IBridge>();
    
    void Awake()
    {
        var env = LuaEnvManager.Get();
        var script = env.DoString($"return require('{luaScriptName}')")[0] as LuaTable;
        luaInstance = script.Get<LuaFunction>("New").Call(this.gameObject)[0] as LuaTable;

        // 获取所有生命周期函数
        onEnableFunc = luaInstance.Get<LuaFunction>("OnEnable");
        onDisableFunc = luaInstance.Get<LuaFunction>("OnDisable");
        updateFunc = luaInstance.Get<LuaFunction>("Update");
        startFunc = luaInstance.Get<LuaFunction>("Start");
        fixedUpdateFunc = luaInstance.Get<LuaFunction>("FixedUpdate");
        lateUpdateFunc = luaInstance.Get<LuaFunction>("LateUpdate");
        onDestroyFunc = luaInstance.Get<LuaFunction>("OnDestroy");

        // 初始化桥接组件
        InitializeBridges();
        
        // 手动触发 Lua Awake
        var awakeFunc = luaInstance.Get<LuaFunction>("Awake");
        awakeFunc?.Call(luaInstance);
    }

    void Start()
    {
        startFunc?.Call(luaInstance);
    }
    
    void OnEnable()
    {
        onEnableFunc?.Call(luaInstance);
    }

    void OnDisable()
    {
        onDisableFunc?.Call(luaInstance);
    }

    void Update()
    {
        updateFunc?.Call(luaInstance, Time.deltaTime);
    }
    
    void FixedUpdate()
    {
        fixedUpdateFunc?.Call(luaInstance, Time.fixedDeltaTime);
    }
    
    void LateUpdate()
    {
        lateUpdateFunc?.Call(luaInstance);
    }
    
    private void InitializeBridges()
    {
        // 获取当前GameObject上所有桥接组件
        var bridgeComponents = GetComponents<IBridge>();
        foreach (var bridge in bridgeComponents)
        {
            bridge.Initialize(luaInstance);
            bridges.Add(bridge);
        }
    }
    
    void OnDestroy()
    {
        onDestroyFunc?.Call(luaInstance);
        
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
