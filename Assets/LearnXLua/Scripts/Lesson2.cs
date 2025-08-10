using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using XLua;

public class Lesson2 : MonoBehaviour
{
    public LuaInit luaInit;
    
    void Start()
    {
        //基础数据类型交换
        Debug.Log("基础数据类型交换");
        luaInit.luaEnv.DoString("require 'Lesson2_data'");
        int num = luaInit.luaEnv.Global.Get<int>("num");
        string str = luaInit.luaEnv.Global.Get<string>("str");
        bool isActive = luaInit.luaEnv.Global.Get<bool>("isActive");
        
        Debug.Log($"From Lua num:{num}, str:{str}, isActive:{isActive}");
        
        //改数据
        luaInit.luaEnv.Global.Set("num", 100);
        luaInit.luaEnv.Global.Set("str", "Changed from C#");
        luaInit.luaEnv.Global.Set("isActive", false);

        luaInit.luaEnv.DoString("print('Lua sees:',num, str, isActive)");
        
        //Lua表 -- C# List
        Debug.Log("Lua表 -- C# List");
        luaInit.luaEnv.DoString("require 'Lesson2_data'");
        List<int> nums = luaInit.luaEnv.Global.Get<List<int>>("nums");
        foreach (var i in nums)
        {
            Debug.Log("From Lua nums" + i);
        }
        //发一个列表给Lua
        List<string> names = new List<string>() { "Tom", "Jack", "Lucy" };
        luaInit.luaEnv.Global.Set("names", names);
        luaInit.luaEnv.DoString(@"
            for i, v in pairs(names) do
                print('From C# names', v)
            end
        ");
        
        //Lua表 -- C# Dictionary
        Debug.Log("Lua表 -- C# Dictionary");
        luaInit.luaEnv.DoString("require 'Lesson2_data'");
        Dictionary<string,int>scores = luaInit.luaEnv.Global.Get<Dictionary<string,int>>("scores");
        foreach (var i in scores)
        {
            Debug.Log($"From Lua scores {i.Key} {i.Value}");
        }

        Dictionary<string, bool> flags = new Dictionary<string, bool>
        {
            { "isReady", true },
            { "isPaused", false }
        };
        luaInit.luaEnv.Global.Set("flags", flags);
        luaInit.luaEnv.DoString(@"
            for k, v in pairs(flags) do
                print('From C# flags', k, v)
            end
        ");
        
        //Lua函数 -- C# Action
        Debug.Log("Lua函数 -- C# Action");
        luaInit.luaEnv.DoString("require 'Lesson2_data'");
        Func<int, int,int> multiply = luaInit.luaEnv.Global.Get<Func<int,int,int>>("multiply");
        int result = multiply(2, 3);
        Debug.Log("From Lua multiply result:" + result);
        //C#发一个Action给Lua
        Action<string> csAction = str => Debug.Log("From C# Action:" + str);
        luaInit.luaEnv.Global.Get<Action<Action<string>>>("call_csharp")?.Invoke(csAction);
        
        //LuaGameObject -- C# GameObject
        Debug.Log("LuaGameObject -- C# GameObject");
        GameObject gameObject = GameObject.Find("TestGameObject");
        luaInit.luaEnv.DoString("require 'Lesson2_data'");
        luaInit.luaEnv.Global.Get<Action<GameObject>>("move_object")?.Invoke(gameObject);
        Debug.Log("Lua move_object result:" + gameObject.transform.position);
    }
    
}