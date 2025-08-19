local LogUtility = {}

LogUtility.LogLevel = {
    Error = 0,
    Warning = 1,
    Info = 2,    
}

LogUtility.LogLayer = {
    Core = 0,
    Framework = 1,
    Game = 2,
    Global = 3,
    Custom = 4,
}

-- 格式化打印
local function format_message(layer, source, level, msg)
    local levelStr = ({ "Info", "Warning", "Error" })[level + 1] or "Unknown"
    local layerStr = ({ "Core", "Framework", "Game","Global","Custom" })[layer + 1] or "Unknown"
    return string.format("[Layer:%s][LuaScript:%s][%s] %s", layerStr, source, levelStr, msg)
end

-- 打印核心方法
function LogUtility.Print(layer, source, level, msg)
    local formatted = format_message(layer, source, level, msg)

    if level == LogUtility.LogLevel.Info then
        print(formatted)
    elseif level == LogUtility.LogLevel.Warning then
        print("<color=yellow>" .. formatted .. "</color>")
    elseif level == LogUtility.LogLevel.Error then
        print("<color=red>" .. formatted .. "</color>")
    end

    -- 如果接入到 C# 日志（如持久化、文件记录），可调用绑定方法
    -- if CS and CS.LogUtility then
    --    CS.LogUtility.LogFromLua(layer, source, level, msg)
    --end
end

-- 快捷方法
function LogUtility.Info(layer, source, msg)
    LogUtility.Print(layer, source, LogUtility.LogLevel.Info, msg)
end

function LogUtility.Warning(layer, source, msg)
    LogUtility.Print(layer, source, LogUtility.LogLevel.Warning, msg)
end

function LogUtility.Error(layer, source, msg)
    LogUtility.Print(layer, source, LogUtility.LogLevel.Error, msg)
end

return LogUtility