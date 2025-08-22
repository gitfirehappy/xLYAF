using System;
using UnityEngine;
using XLua;

public class Lesson6_Event : MonoBehaviour
{
    public LuaInit luaInit;
    
    // C# 事件
    public static event Action<string> OnCSharpEvent;
    
    // Lua 回调
    private Action<string> luaCallback;
    
    void Start()
    {
        luaInit.luaEnv.DoString("require 'Lesson6_Event'");
        
        // 获取Lua回调函数
        luaCallback = luaInit.luaEnv.Global.Get<Action<string>>("on_lua_callback");
        
        // 注册Lua到C#的事件监听
        OnCSharpEvent += (message) => {
            Debug.Log($"C# received from Lua: {message}");
        };
    }
    
    [ContextMenu("1. Lua触发事件-Lua接收")]
    void TestLuaToLua()
    {
        luaInit.luaEnv.Global.Get<Action>("test_lua_to_lua")?.Invoke();
    }
    
    [ContextMenu("2. C#触发事件-C#接收")]
    void TestCSharpToCSharp()
    {
        Debug.Log("C# triggering event for C#");
        OnCSharpEvent?.Invoke("C# to C# message");
    }
    
    [ContextMenu("3. Lua触发事件-C#接收")]
    void TestLuaToCSharp()
    {
        luaInit.luaEnv.Global.Get<Action>("test_lua_to_csharp")?.Invoke();
    }
    
    [ContextMenu("4. C#触发事件-Lua接收")]
    void TestCSharpToLua()
    {
        Debug.Log("C# triggering event for Lua");
        luaCallback?.Invoke("C# to Lua message");
    }
    
    void OnDestroy()
    {
        luaCallback = null;
    }
    
    // 供Lua调用的方法
    public static void SendToCSharp(string message)
    {
        Debug.Log($"C# received from Lua: {message}");
    }
}