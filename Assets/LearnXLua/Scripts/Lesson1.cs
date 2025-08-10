using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class Lesson1 : MonoBehaviour
{
    public LuaInit luaInit;
    
    void Start()
    {
        //Hello Lua
        luaInit.luaEnv.DoString("require 'Lesson1_Hello'");    
        //lua中调用unityAPI
        luaInit.luaEnv.DoString("require 'Lesson1_lua_call_unity'");
        //C#调Lua函数并获取返回值
        luaInit.luaEnv.DoString("require 'Lesson1_lua_functions'");
        Func<int,int,int> add = luaInit.luaEnv.Global.Get<Func<int,int,int>>("add");
        int result = add(10, 20);
        Debug.Log("Lua add result" + result);
        //Lua操作GameObject
        luaInit.luaEnv.DoString("require 'Lesson1_GameObject_control'");
    }
}
