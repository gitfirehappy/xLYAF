using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public static class LuaCoroutineScheduler
{
    public class LuaCoroutine
    {
        public int Id;
        public object Coroutine;
        public string Name;
        public bool IsRunning;
        public Action OnCompleted;
    }

    private static readonly Dictionary<int, LuaCoroutine> _luaCoroutines = new();
    private static int _idCounter;
    private static LuaEnv _luaEnv;
    
    // 初始化时预加载避免每次调用查找
    private static LuaFunction _coroutineResume;
    private static LuaFunction _coroutineStatus;
    private static LuaFunction _coroutineCreate;

    public static void Init(LuaEnv luaEnv)
    {
        _luaEnv = luaEnv ?? throw new ArgumentNullException(nameof(luaEnv), "LuaEnv cannot be null");
    
        // 先获取coroutine全局表
        LuaTable coroutineTable = _luaEnv.Global.Get<LuaTable>("coroutine");
        if (coroutineTable == null)
            throw new InvalidOperationException("Failed to get 'coroutine' table from Lua environment");

        // 从coroutine表中获取具体函数
        _coroutineResume = coroutineTable.Get<LuaFunction>("resume");
        _coroutineStatus = coroutineTable.Get<LuaFunction>("status");
        _coroutineCreate = coroutineTable.Get<LuaFunction>("create");

        // 检查关键函数是否获取成功
        if (_coroutineResume == null)
            throw new InvalidOperationException("Failed to get 'coroutine.resume' from Lua environment");
        if (_coroutineStatus == null)
            throw new InvalidOperationException("Failed to get 'coroutine.status' from Lua environment");
        if (_coroutineCreate == null)
            throw new InvalidOperationException("Failed to get 'coroutine.create' from Lua environment");
    }

    /// <summary>
    /// 启动Lua协程
    /// </summary>
    public static int Start(LuaFunction func, string name = "Unnamed")
    {
        if (_luaEnv == null)
        {
            Debug.LogError("[LuaCoroutineScheduler] LuaEnv not initialized. Call Init() first.");
            return -1;
        }
        // 补充检查核心函数是否初始化
        if (_coroutineCreate == null)
        {
            Debug.LogError("[LuaCoroutineScheduler] Coroutine functions not initialized properly");
            return -1;
        }
        if (func == null)
        {
            Debug.LogError("[LuaCoroutineScheduler] LuaFunction cannot be null");
            return -1;
        }
        
        int id = ++_idCounter;
    
        // 使用全局协程创建函数
        object[] createResult;
        try
        {
            createResult = _coroutineCreate.Call(func);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LuaCoroutineScheduler] Failed to create coroutine '{name}': {ex}");
            return -1;
        }
        
        if (createResult == null || createResult.Length == 0 || createResult[0] == null)
        {
            Debug.LogError($"[LuaCoroutineScheduler] Failed to create coroutine '{name}'");
            //if(createResult[0] == null)Debug.LogError(createResult[0]);
            return -1;
        }
        
        _luaCoroutines[id] = new LuaCoroutine
        {
            Id = id,
            Coroutine = createResult[0],
            Name = name,
            IsRunning = true
        };
        Resume(id); // 初始执行
        return id;
    }

    /// <summary>
    /// 恢复Lua协程执行
    /// </summary>
    public static void Resume(int id, params object[] args)
    {
        if (!_luaCoroutines.TryGetValue(id, out var co) || !co.IsRunning)
            return;
    
        // 构造参数：Lua协程对象 + 外部参数
        object[] callArgs = new object[args.Length + 1];
        callArgs[0] = co.Coroutine;
        Array.Copy(args, 0, callArgs, 1, args.Length);

        try
        {
            // 直接调用coroutine.resume
            object[] results = _coroutineResume.Call(callArgs);
        
            // 检查执行结果 (results[0]为bool表示是否成功)
            if (results.Length > 0 && results[0] is bool success && !success)
            {
                string error = results.Length > 1 ? results[1].ToString() : "Unknown error";
                Debug.LogError($"[LuaCoroutineScheduler] Coroutine {co.Name} error: {error}");
                MarkCompleted(id);
                return;
            }
        
            // 检查协程状态
            string status = _coroutineStatus.Call(co.Coroutine)[0] as string;
            if (status == "dead") MarkCompleted(id);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LuaCoroutineScheduler] Critical error in {co.Name}: {ex}");
            MarkCompleted(id);
        }
    }

    /// <summary>
    /// 注册完成回调
    /// </summary>
    public static void RegisterCompletionCallback(int id, Action callback)
    {
        if (_luaCoroutines.TryGetValue(id, out var co))
            co.OnCompleted = callback;
    }

    /// <summary>
    /// 标记协程完成
    /// </summary>
    public static void MarkCompleted(int id)
    {
        if (_luaCoroutines.TryGetValue(id, out var co))
        {
            co.IsRunning = false;
            co.OnCompleted?.Invoke();
            co.OnCompleted = null;
        }
    }

    /// <summary>
    /// 停止Lua协程
    /// </summary>
    public static void Stop(int id)
    {
        if (_luaCoroutines.TryGetValue(id, out var co))
        {
            co.IsRunning = false;
            co.OnCompleted = null;
            _luaCoroutines.Remove(id);
        }
    }

    /// <summary>
    /// 检查协程是否运行中
    /// </summary>
    public static bool IsRunning(int id) => 
        _luaCoroutines.TryGetValue(id, out var co) && co.IsRunning;

    /// <summary>
    /// 清理所有协程
    /// </summary>
    public static void ClearAll()
    {
        foreach (var co in _luaCoroutines.Values)
            co.OnCompleted = null;
        _luaCoroutines.Clear();
    }
}