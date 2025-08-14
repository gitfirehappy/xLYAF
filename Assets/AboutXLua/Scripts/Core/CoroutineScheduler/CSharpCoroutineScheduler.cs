using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CSharpCoroutineScheduler
{
    public class CoroutineInfo
    {
        public int Id;
        public Coroutine Handle;
        public string Name;
        public bool IsRunning;
    }

    private static MonoBehaviour _runner;
    private static readonly Dictionary<int, CoroutineInfo> _csCoroutines = new();
    private static int _idCounter;

    public static void Init(MonoBehaviour runner) => _runner = runner;

    /// <summary>
    /// 启动C#协程
    /// </summary>
    public static int Start(IEnumerator routine, string name = "Unnamed")
    {
        if (_runner == null)
        {
            Debug.LogError("[CSharpCoroutineScheduler] Runner not initialized");
            return -1;
        }
        
        int id = ++_idCounter;
        var handle = _runner.StartCoroutine(WrappedCoroutine(id, routine));
        
        _csCoroutines[id] = new CoroutineInfo
        {
            Id = id,
            Handle = handle,
            Name = name,
            IsRunning = true
        };
        
        return id;
    }
    
    private static IEnumerator WrappedCoroutine(int id, IEnumerator routine)
    {
        while (true)
        {
            bool moved;
            try
            {
                moved = routine.MoveNext();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CSharpCoroutineScheduler] Error in '{_csCoroutines[id].Name}': {ex}");
                break;
            }

            if (!moved) break; // 协程结束
            
            yield return routine.Current; // 安全地在try外使用yield return
        }

        // 标记完成
        if (_csCoroutines.TryGetValue(id, out var info))
        {
            info.IsRunning = false;
        }
    }

    /// <summary>
    /// 停止C#协程
    /// </summary>
    public static void Stop(int id)
    {
        if (_csCoroutines.TryGetValue(id, out var info) && info.IsRunning)
        {
            _runner.StopCoroutine(info.Handle);
            _csCoroutines.Remove(id);
        }
    }

    /// <summary>
    /// 检查协程是否运行中
    /// </summary>
    public static bool IsRunning(int id) => 
        _csCoroutines.TryGetValue(id, out var info) && info.IsRunning;

    /// <summary>
    /// 清理所有协程
    /// </summary>
    public static void ClearAll()
    {
        foreach (var info in _csCoroutines.Values)
        {
            if (info.IsRunning)
                _runner.StopCoroutine(info.Handle);
        }
        _csCoroutines.Clear();
    }
}