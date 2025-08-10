using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class Lesson4 : MonoBehaviour
{
    public LuaInit luaInit;

    private Action eventA;
    private Action<string> eventB;
    private Action<string> eventChain;
    private Action<Person> usePerson;
    private LuaDelegateConfig.Action_return_Vector3 createVector3;
    
    private LuaFunction startCoroutine;
    private LuaFunction resumeCoroutine;
    private object luaCoroutine;
    
    private Action<int> fastLogTest;

    void Start()
    {
        //载入Lua
        luaInit.luaEnv.DoString("require 'Lesson4'");
        
        //委托绑定进阶
        eventA = luaInit.luaEnv.Global.Get<Action>("on_event_a");
        eventB = luaInit.luaEnv.Global.Get<Action<string>>("on_event_b");
        eventChain = luaInit.luaEnv.Global.Get<Action<string>>("on_event_chain");
        
        //模拟事件触发
        eventA?.Invoke();
        eventB?.Invoke("Hello from C#");
        eventChain += msg => Debug.Log("C# extra chain: " + msg);
        eventChain?.Invoke("Chained Call");
        
        //Lua--C#类与结构体交互
        usePerson = luaInit.luaEnv.Global.Get<Action<Person>>("use_person");
        createVector3 = luaInit.luaEnv.Global.Get<LuaDelegateConfig.Action_return_Vector3>("create_vector3");
        
        Person person = new Person{Name = "Tom", Age = 18};
        usePerson?.Invoke(person);
        Debug.Log($"C#: Person after Lua modify: Name = {person.Name}, Age = {person.Age}");
        
        Vector3 v3 = createVector3();
        Debug.Log($"C#: Vector3 from Lua: {v3}");
        
        //Lua协程
        startCoroutine = luaInit.luaEnv.Global.Get<LuaFunction>("start_coroutine");
        resumeCoroutine = luaInit.luaEnv.Global.Get<LuaFunction>("resume_coroutine");
        StartCoroutine(RunLuaCoroutine());
        
        //性能优化
        fastLogTest = luaInit.luaEnv.Global.Get<Action<int>>("fast_log_test");
        fastLogTest?.Invoke(3);
    }
    
    IEnumerator RunLuaCoroutine()
    {
        // 启动协程
        var startResult = startCoroutine.Call();
        if (startResult == null || startResult.Length == 0 || !(bool)startResult[0])
        {
            yield break;
        }
    
        // 第一次等待
        yield return new WaitForSeconds(1f);
    
        // 恢复协程
        resumeCoroutine.Call();
    
        // 第二次等待
        yield return new WaitForSeconds(2f);
    
        // 恢复协程
        resumeCoroutine.Call();
    }
}

[LuaCallCSharp]
public class Person
{
    public string Name;
    public int Age;
}
