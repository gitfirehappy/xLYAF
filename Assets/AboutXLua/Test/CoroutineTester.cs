using System;
using System.Collections;
using UnityEngine;
using XLua;

public class CoroutineTester : MonoBehaviour
{
    public bool runAllTestsOnStart = true;
    private LuaEnv _luaEnv;
    private bool _testInProgress;

    void Start()
    {
        _luaEnv = new LuaEnv();
        CSharpCoroutineScheduler.Init(this);
        LuaCoroutineScheduler.Init(_luaEnv);
        
        if (runAllTestsOnStart)
            StartCoroutine(RunAllTests());
    }

    IEnumerator RunAllTests()
    {
        Debug.Log("===== 开始执行所有测试 =====");
        yield return TestBasicCSharpCoroutine();
        yield return TestBasicLuaCoroutine();
        yield return TestCSharpWaitForLua();
        yield return TestLuaWaitForCSharp();
        yield return TestCoroutineStop();
        yield return TestExceptionHandling();
        yield return TestNestedCoroutines();
        Debug.Log("✅ 所有测试完成");
        _testInProgress = false;
    }

    void OnDestroy()
    {
        _luaEnv?.Dispose();
        CSharpCoroutineScheduler.ClearAll();
        LuaCoroutineScheduler.ClearAll();
    }

    #region 测试方法（可通过ContextMenu单独运行）
    [ContextMenu("测试1: 基础C#协程")]
    public void Test1_BasicCSharpCoroutine() => StartCoroutine(TestBasicCSharpCoroutine());
    
    [ContextMenu("测试2: 基础Lua协程")]
    public void Test2_BasicLuaCoroutine() => StartCoroutine(TestBasicLuaCoroutine());
    
    [ContextMenu("测试3: C#等待Lua")]
    public void Test3_CSharpWaitForLua() => StartCoroutine(TestCSharpWaitForLua());
    
    [ContextMenu("测试4: Lua等待C#")]
    public void Test4_LuaWaitForCSharp() => StartCoroutine(TestLuaWaitForCSharp());
    
    [ContextMenu("测试5: 协程停止")]
    public void Test5_CoroutineStop() => StartCoroutine(TestCoroutineStop());
    
    [ContextMenu("测试6: 异常处理")]
    public void Test6_ExceptionHandling() => StartCoroutine(TestExceptionHandling());
    
    [ContextMenu("测试7: 嵌套协程")]
    public void Test7_NestedCoroutines() => StartCoroutine(TestNestedCoroutines());
    
    [ContextMenu("运行所有测试")]
    public void RunAllTestsMenu() => StartCoroutine(RunAllTests());
    #endregion

    #region 测试实现
    IEnumerator TestBasicCSharpCoroutine()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试1: 基础C#协程 =====");
        bool completed = false;
        int id = CSharpCoroutineScheduler.Start(SimpleCSharpCoroutine(() => completed = true), "SimpleCSharp");
        
        yield return new WaitUntil(() => completed);
        Debug.Log($"✅ 测试1完成 | 状态: {!CSharpCoroutineScheduler.IsRunning(id)}\n");
        _testInProgress = false;
    }

    IEnumerator SimpleCSharpCoroutine(System.Action onComplete)
    {
        Debug.Log("C#协程: 开始");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("C#协程: 完成");
        onComplete?.Invoke();
    }

    IEnumerator TestBasicLuaCoroutine()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试2: 基础Lua协程 =====");
        bool completed = false;
        var luaCode = @"
        print('[Lua] 开始')
        -- 使用UnityEngine.WaitForSeconds等待
        coroutine.yield(CS.UnityEngine.WaitForSeconds(0.1))
        print('[Lua] 完成')
    ";
        
        LuaFunction func;
        try
        {
            var result = _luaEnv.DoString($"return function() {luaCode} end");
            if (result == null || result.Length == 0 || !(result[0] is LuaFunction))
            {
                Debug.LogError("Failed to create Lua function");
                yield break;
            }
            func = result[0] as LuaFunction;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Lua function creation failed: {ex}");
            yield break;
        }
        
        int id = LuaCoroutineScheduler.Start(func, "SimpleLua");
        
        LuaCoroutineScheduler.RegisterCompletionCallback(id, () => completed = true);
        yield return new WaitUntil(() => completed);
        Debug.Log($"✅ 测试2完成 | 状态: {!LuaCoroutineScheduler.IsRunning(id)}\n");
        _testInProgress = false;
    }

    IEnumerator TestCSharpWaitForLua()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试3: C#等待Lua =====");
        var luaCode = @"
            print('[Lua] 开始执行')
            coroutine.yield(0.2)
            print('[Lua] 即将完成')
        ";
        
        var func = _luaEnv.DoString($"return function() {luaCode} end")[0] as LuaFunction;
        int id = LuaCoroutineScheduler.Start(func, "WaitableLua");
        
        Debug.Log("C#开始等待Lua...");
        yield return CoroutineBridge.CSharpWaitForLuaTask(id);
        Debug.Log($"✅ 测试3完成 | Lua状态: {!LuaCoroutineScheduler.IsRunning(id)}\n");
        _testInProgress = false;
    }

    IEnumerator TestLuaWaitForCSharp()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试4: Lua等待C# =====");
        bool luaResumed = false;
        var luaCode = @"
            print('[Lua] 开始执行')
            CoroutineBridge.LuaWaitForCSharpTask(coroutine.running(), csCoroutineId)
            coroutine.yield()  -- 等待C#完成
            print('[Lua] 恢复执行')
            luaResumed = true
        ";
        
        int csId = CSharpCoroutineScheduler.Start(WaitableCSharpCoroutine(), "WaitableCSharp");
        _luaEnv.Global.Set("csCoroutineId", csId);
        _luaEnv.Global.Set("luaResumed", false);
        
        var func = _luaEnv.DoString($"return function() {luaCode} end")[0] as LuaFunction;
        int luaId = LuaCoroutineScheduler.Start(func, "LuaWaiting");
        
        yield return new WaitUntil(() => _luaEnv.Global.Get<bool>("luaResumed"));
        Debug.Log($"✅ 测试4完成 | C#状态: {!CSharpCoroutineScheduler.IsRunning(csId)}\n");
        _testInProgress = false;
    }

    IEnumerator WaitableCSharpCoroutine()
    {
        Debug.Log("C#协程(供Lua等待): 开始");
        yield return new WaitForSeconds(0.3f);
        Debug.Log("C#协程(供Lua等待): 完成");
    }

    IEnumerator TestCoroutineStop()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试5: 协程停止 =====");
        // C#协程停止
        bool csStopped = false;
        int csId = CSharpCoroutineScheduler.Start(RunForeverCoroutine(() => csStopped = true), "StoppableCSharp");
        yield return new WaitForSeconds(0.2f);
        CSharpCoroutineScheduler.Stop(csId);
        Debug.Log($"C#停止结果: {!CSharpCoroutineScheduler.IsRunning(csId)} | 回调: {csStopped}");
        
        // Lua协程停止
        bool luaStopped = false;
        var luaFunc = _luaEnv.DoString("return function() while true do coroutine.yield(0.1) end end")[0] as LuaFunction;
        int luaId = LuaCoroutineScheduler.Start(luaFunc, "StoppableLua");
        LuaCoroutineScheduler.RegisterCompletionCallback(luaId, () => luaStopped = true);
        
        yield return new WaitForSeconds(0.2f);
        LuaCoroutineScheduler.Stop(luaId);
        Debug.Log($"Lua停止结果: {!LuaCoroutineScheduler.IsRunning(luaId)} | 回调: {luaStopped}");
        Debug.Log("✅ 测试5完成\n");
        _testInProgress = false;
    }

    IEnumerator RunForeverCoroutine(System.Action onStop)
    {
        try {
            while (true) {
                yield return new WaitForSeconds(0.1f);
                Debug.Log("持续运行的协程...");
            }
        } finally {
            onStop?.Invoke();
        }
    }

    IEnumerator TestExceptionHandling()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试6: 异常处理 =====");
        // C#异常
        int csId = CSharpCoroutineScheduler.Start(ErrorCSharpCoroutine(), "ErrorCSharp");
        yield return new WaitForSeconds(0.1f);
        Debug.Log($"C#异常处理: {!CSharpCoroutineScheduler.IsRunning(csId)}");
        
        // Lua异常
        var luaCode = @"error('Lua测试错误')";
        var func = _luaEnv.DoString($"return function() {luaCode} end")[0] as LuaFunction;
        int luaId = LuaCoroutineScheduler.Start(func, "ErrorLua");
        yield return new WaitForSeconds(0.1f);
        Debug.Log($"Lua异常处理: {!LuaCoroutineScheduler.IsRunning(luaId)}");
        Debug.Log("✅ 测试6完成\n");
        _testInProgress = false;
    }

    IEnumerator ErrorCSharpCoroutine()
    {
        yield return null;
        throw new System.Exception("C#测试错误");
    }

    IEnumerator TestNestedCoroutines()
    {
        if (CheckTestInProgress()) yield break;
        
        Debug.Log("===== 测试7: 嵌套协程 =====");
        bool completed = false;
        int csId = CSharpCoroutineScheduler.Start(NestedCSharpCoroutine(() => completed = true), "NestedCSharp");
        yield return new WaitUntil(() => completed);
        Debug.Log($"✅ 测试7完成 | 状态: {!CSharpCoroutineScheduler.IsRunning(csId)}\n");
        _testInProgress = false;
    }

    IEnumerator NestedCSharpCoroutine(System.Action onComplete)
    {
        Debug.Log("外层C#开始");
        
        var luaCode = @"
            print('[Lua] 内层Lua开始')
            coroutine.yield(0.2)
            print('[Lua] 内层Lua结束')
        ";
        var func = _luaEnv.DoString($"return function() {luaCode} end")[0] as LuaFunction;
        int luaId = LuaCoroutineScheduler.Start(func, "NestedLua");
        
        yield return CoroutineBridge.CSharpWaitForLuaTask(luaId);
        Debug.Log("外层C#恢复");
        onComplete?.Invoke();
    }
    #endregion

    private bool CheckTestInProgress()
    {
        if (_testInProgress)
        {
            Debug.LogWarning("测试进行中，请等待当前测试完成");
            return true;
        }
        
        _testInProgress = true;
        return false;
    }
}