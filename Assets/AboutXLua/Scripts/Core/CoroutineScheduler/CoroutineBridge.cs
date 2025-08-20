using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;
using XLua;

public static class CoroutineBridge
{
    /// <summary> Lua协程等待C#协程的映射表 [LuaCoID] = C#CoID </summary>
    private static readonly Dictionary<int, int> _luaWaitingForCSharp = new();
    
    /// <summary> C#协程等待Lua协程的映射表 [C#CoID] = LuaCoID </summary>
    private static readonly Dictionary<int, int> _csharpWaitingForLua = new();
    
    /// <summary> C#协程的Lua等待者 [C#CoID] = List《LuaCoID》 </summary>
    private static readonly Dictionary<int, List<int>> _csharpToLuaWaiters = new();
    
    /// <summary> Lua协程的C#等待者 [LuaCoID] = List《C#CoID》</summary>
    private static readonly Dictionary<int, List<int>> _luaToCSharpWaiters = new();
    
    
    /// <summary>
    /// 清理与指定Lua协程相关的所有等待关系
    /// </summary>
    public static void CleanupWaitRelations(int luaCoId)
    {
        // 清理Lua等待C#的关系
        if (_luaWaitingForCSharp.ContainsKey(luaCoId))
        {
            _luaWaitingForCSharp.Remove(luaCoId);
        }
        
        // 清理C#等待Lua的关系
        if (_luaToCSharpWaiters.TryGetValue(luaCoId, out var csWaiters))
        {
            foreach (var csCoId in csWaiters)
            {
                _csharpWaitingForLua.Remove(csCoId);
            }
            _luaToCSharpWaiters.Remove(luaCoId);
        }
    }
    
    /// <summary>
    /// Lua协程等待C#协程
    /// </summary>
    public static void LuaWaitForCSharp(int luaCoId, int csCoId)
    {
        if (csCoId <= 0) {
            LogUtility.Error(LogLayer.Core, "CoroutineBridge", 
                $"Invalid C# Coroutine ID: {csCoId}");
            return;
        }
        
        _luaWaitingForCSharp[luaCoId] = csCoId;
        
        // 建立反向映射
        if (!_csharpToLuaWaiters.TryGetValue(csCoId, out var list))
        {
            list = new List<int>();
            _csharpToLuaWaiters[csCoId] = list;
        }
        list.Add(luaCoId);
        LogUtility.Info(LogLayer.Core, "CoroutineBridge", 
            $"Lua#{luaCoId} 开始等待 C##{csCoId}");
    }
    
    /// <summary>
    /// C#协程等待Lua协程
    /// </summary>
    public static void CSharpWaitForLua(int csCoId, int luaCoId)
    {
        if (luaCoId <= 0) {
            LogUtility.Error(LogLayer.Core, "CoroutineBridge", 
                $"Invalid Lua Coroutine ID: {luaCoId}");
            return;
        }
        
        _csharpWaitingForLua[csCoId] = luaCoId;
        
        if (!_luaToCSharpWaiters.TryGetValue(luaCoId, out var list))
        {
            list = new List<int>();
            _luaToCSharpWaiters[luaCoId] = list;
        }
        list.Add(csCoId);
        LogUtility.Info(LogLayer.Core, "CoroutineBridge", 
            $"C##{csCoId} 开始等待 Lua#{luaCoId}");
    }
    
    /// <summary>
    /// 通知C#协程完成
    /// </summary>
    public static void NotifyCSharpComplete(int csCoId)
    {
        LuaEnv luaEnv = null;
        try
        {
            // 使用LuaEnvManager直接获取Lua环境
            luaEnv = LuaEnvManager.Get();
        }
        catch (Exception ex)
        {
            LogUtility.Error(LogLayer.Core, "CoroutineBridge", 
                $"Failed to get LuaEnv: {ex.Message}");
            return;
        }
        
        if (luaEnv == null) {
            LogUtility.Error(LogLayer.Core, "CoroutineBridge", 
                "LuaEnv is null!");
            return;
        }
        
        // 通知所有等待此C#协程的Lua协程
        if (_csharpToLuaWaiters.TryGetValue(csCoId, out var luaWaiters))
        {
            LogUtility.Info(LogLayer.Core, "CoroutineBridge", 
                $"C##{csCoId} 完成，通知 {luaWaiters.Count} 个Lua协程恢复");
            foreach (var luaCoId in luaWaiters)
            {
                _luaWaitingForCSharp.Remove(luaCoId);
                // 恢复等待的Lua协程
                LuaCoroutineScheduler.Resume(luaCoId, luaEnv);
                LogUtility.Info(LogLayer.Core, "CoroutineBridge", 
                    $"尝试恢复 Lua#{luaCoId}");
            }
            _csharpToLuaWaiters.Remove(csCoId);
        }
        else
        {
            LogUtility.Warning(LogLayer.Core, "CoroutineBridge", 
                $"[CoroutineBridge] C##{csCoId} 完成，但无等待的Lua协程");
        }
    }
    
    /// <summary>
    /// 通知Lua协程完成
    /// </summary>
    public static void NotifyLuaComplete(int luaCoId)
    {
        // 通知所有等待此Lua协程的C#协程
        if (_luaToCSharpWaiters.TryGetValue(luaCoId, out var csWaiters))
        {
            LogUtility.Info(LogLayer.Core, "CoroutineBridge", 
                $" Lua#{luaCoId} 完成，通知 {csWaiters.Count} 个C#协程恢复");
            foreach (var csCoId in csWaiters)
            {
                _csharpWaitingForLua.Remove(csCoId);
            }
            _luaToCSharpWaiters.Remove(luaCoId);
        }
        else
        {
            LogUtility.Warning(LogLayer.Core, "CoroutineBridge", 
                $"Lua#{luaCoId} 完成，但无等待的C#协程");
        }
    }

    /// <summary>
    /// C#协程等待Lua协程的协程方法
    /// </summary>
    public static IEnumerator WaitForLuaCoroutine(int luaCoId)
    {
        int csCoId = CSharpCoroutineScheduler.GetCurrentCoroutineId();
        if (csCoId == -1)
        {
           LogUtility.Warning(LogLayer.Core, "CoroutineBridge", 
               "CSharpWaitForLua called outside of a coroutine");
            yield break;
        }
        
        CSharpWaitForLua(csCoId, luaCoId);
        
        // 等待直到Lua协程完成
        while (_csharpWaitingForLua.ContainsKey(csCoId))
        {
            yield return null;
        }
    }
}