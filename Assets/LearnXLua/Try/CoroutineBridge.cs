using System;
using System.Collections;
using UnityEngine;

public static class CoroutineBridge
{
    /// <summary>
    /// Lua等待C#协程完成
    /// </summary>
    public static void LuaWaitForCSharpTask(int luaCoroutineId, IEnumerator csRoutine)
    {
        CSharpCoroutineScheduler.Start(
            WaitAndResumeLua(luaCoroutineId, csRoutine),
            $"LuaWaitForCSharp_{luaCoroutineId}"
        );
    }

    private static IEnumerator WaitAndResumeLua(int luaCoroutineId, IEnumerator csRoutine)
    {
        // 等待C#协程完成后恢复Lua协程
        yield return CSharpCoroutineScheduler.Start(csRoutine, "NestedCSharp");
        LuaCoroutineScheduler.Resume(luaCoroutineId);
    }

    /// <summary>
    /// C#等待Lua协程完成
    /// </summary>
    public static IEnumerator CSharpWaitForLuaTask(int luaCoroutineId)
    {
        var completionSource = new TaskCompletionSource();
        
        // 注册完成回调
        LuaCoroutineScheduler.RegisterCompletionCallback(luaCoroutineId, completionSource.SetResult);
        
        // 启动Lua协程
        LuaCoroutineScheduler.Resume(luaCoroutineId);
        
        // 等待Lua协程完成
        while (!completionSource.IsCompleted)
        {
            yield return null;
        }
    }

    // 简化的非泛型完成源（仅关注完成状态）
    private class TaskCompletionSource
    {
        public bool IsCompleted { get; private set; }
        
        public void SetResult() => IsCompleted = true;
    }
}