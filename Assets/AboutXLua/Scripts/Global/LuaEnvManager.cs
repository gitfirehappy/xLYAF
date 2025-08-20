using System;
using UnityEngine;
using XLua;

public static class LuaEnvManager
{
    private static LuaEnv _env;

    public static void CreateNewEnv()
    {
        Dispose(); // 清理现有环境
        _env = new LuaEnv();
        LogUtility.Info(LogLayer.Global,"LuaEnvManager",
            "Created new Lua environment");
    }

    public static void Set(LuaEnv env)
    {
        Dispose(); // 清理现有环境
        _env = env;
        LogUtility.Info(LogLayer.Global,"LuaEnvManager",
            "Set existing Lua environment");
    }

    public static LuaEnv Get()
    {
        if (_env == null)
            throw new Exception("LuaEnv has not been initialized. Call CreateNewEnv() or Set() first.");
        return _env;
    }

    public static bool IsReady => _env != null;

    public static void Dispose()
    {
        if (_env != null)
        {
            _env.Dispose();
            _env = null;
            LogUtility.Info(LogLayer.Global,"LuaEnvManager",
                "Disposed Lua environment");
        }
    }
}