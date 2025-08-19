using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

public static class LogUtility
{
    private static List<LogEntry> _logEntries = new List<LogEntry>();
    private static readonly object _lockObj = new object();
    
    public static IReadOnlyList<LogEntry> LogEntries 
    { 
        get 
        { 
            lock (_lockObj)
            {
                return new List<LogEntry>(_logEntries);
            }
        } 
    }
    
    public static event Action OnLogAdded;
    
    public static void ClearLogs()
    {
        lock (_lockObj)
        {
            _logEntries.Clear();
        }
    }
    
    public static void Log(LogLayer layer, string source, LogLevel level, string message)
    {
        string formatted = $"[Layer:{layer}][Script:{source}][{level}] {message}";

        // 记录日志到列表
        lock (_lockObj)
        {
            _logEntries.Add(new LogEntry
            {
                ScriptType = "CSharp",
                Layer = layer,
                Level = level,
                Source = source,
                Message = message,
                FormattedMessage = formatted,
                Time = DateTime.Now
            });
        }
        OnLogAdded?.Invoke();
        
        switch (level)
        {
            case LogLevel.Info:
                UnityEngine.Debug.Log(formatted);
                break;
            case LogLevel.Warning:
                UnityEngine.Debug.LogWarning(formatted);
                break;
            case LogLevel.Error:
                UnityEngine.Debug.LogError(formatted);
                break;
        }
    }
    
    // 用于 Lua 调用（需绑定到 LuaCallCSharp）
    public static void LogFromLua(int layer, string source, int level, string message)
    {
        if (!Enum.IsDefined(typeof(LogLayer), layer)) layer = (int)LogLayer.Custom;
        if (!Enum.IsDefined(typeof(LogLevel), level)) level = (int)LogLevel.Info;
        
        var logLayer = (LogLayer)layer;
        var logLevel = (LogLevel)level;
        var fullSource = "Lua:" + source;
        string formatted = $"[Layer:{logLayer}][Script:{fullSource}][{logLevel}] {message}";

        // 记录Lua日志到列表
        lock (_lockObj)
        {
            _logEntries.Add(new LogEntry
            {
                ScriptType = "Lua",
                Layer = logLayer,
                Level = logLevel,
                Source = fullSource,
                Message = message,
                FormattedMessage = formatted,
                Time = DateTime.Now
            });
        }
        OnLogAdded?.Invoke();

        Log(logLayer, fullSource, logLevel, message);
    }
}

public enum LogLevel
{
    Error,
    Warning,
    Info
}

public enum LogLayer {
    Core,       // 底层核心
    Framework,  // 中间层
    Game,         // 上层展示
    Global,      // 全局
    Custom      // 可自定义扩展
}

public class LogEntry
{
    public string ScriptType { get; set; } // Lua或CSharp
    public LogLayer Layer { get; set; }
    public LogLevel Level { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
    public string FormattedMessage { get; set; }
    public DateTime Time { get; set; }
}

