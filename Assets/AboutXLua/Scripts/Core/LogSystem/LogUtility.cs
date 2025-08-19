using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

public static class LogUtility
{
    public static void Log(LogLayer layer, string source, LogLevel level, string message)
    {
        string formatted = $"[Layer:{layer}][Script:{source}][{level}] {message}";

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
        
        Log((LogLayer)layer, "Lua:" + source, (LogLevel)level, message);
    }
}

public enum LogLayer {
    Core,       // 底层核心
    Framework,  // 中间层
    Game,         // 上层展示
    Global,      // 全局
    Custom      // 可自定义扩展
}

