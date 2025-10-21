using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public static class XLuaConfig
{
    // Lua调用C#的静态基础类型
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = new List<Type>()
    {
        typeof(UnityEngine.Object),
        typeof(UnityEngine.GameObject),
        typeof(UnityEngine.Transform),
        typeof(UnityEngine.MonoBehaviour),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Vector3),
        typeof(UnityEngine.Vector2),
        typeof(UnityEngine.Collision2D),  
        typeof(UnityEngine.Collider2D),   
        typeof(UnityEngine.Rigidbody2D),  
        typeof(UnityEngine.Quaternion),
        typeof(UnityEngine.Debug),
        typeof(UnityEngine.Mathf),
        typeof(System.Collections.Generic.List<int>),
        typeof(System.Action),
        typeof(System.Action<int>),
        typeof(System.Func<bool>),

        // 常用框架方法
        typeof(IBridge),
        typeof(LuaBehaviour),
        typeof(Physics2DBridge),
        typeof(Collision2DBridge)
    };

    // C#调用Lua的静态基础类型
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>()
    {
        typeof(System.Action),
        typeof(System.Action<int>),
        typeof(System.Action<string>),
        typeof(System.Func<int, string>),
        typeof(System.Collections.IEnumerator), 
        typeof(System.Func<System.Collections.IEnumerator>),

    };

    // 热修复静态基础类型(几乎不用)
    [Hotfix]
    public static List<Type> Hotfix = new List<Type>()
    {
        
    };
}
