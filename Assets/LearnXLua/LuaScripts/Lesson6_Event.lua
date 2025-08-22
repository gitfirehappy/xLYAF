-- Lua事件表
local luaEventHandlers = {}

-- Lua触发事件，Lua接收
function test_lua_to_lua()
    print("Lua: Triggering event for Lua")
    for _, handler in ipairs(luaEventHandlers) do
        handler("Lua to Lua message")
    end
end

-- Lua触发事件，C#接收
function test_lua_to_csharp()
    print("Lua: Triggering event for C#")
    CS.Lesson6_Event.SendToCSharp("Lua to C# message")
end

-- Lua回调函数（供C#调用）
function on_lua_callback(message)
    print("Lua received from C#: " .. message)
end

-- 添加Lua事件监听器
function add_lua_event_listener(handler)
    table.insert(luaEventHandlers, handler)
end

-- 初始化时添加一个Lua事件监听器
add_lua_event_listener(function(message)
    print("Lua event handler: " .. message)
end)