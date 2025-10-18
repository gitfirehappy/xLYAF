local UIEventUtils = {}
local CSBridgeType = "LuaUIEventBridge"

-- 获取或添加LuaUIEventBridge组件
local function GetOrAddBridge(gameObject)
    if not gameObject then
        CS.Debug.LogError("UIEventUtils: 无效的游戏对象")
        return nil
    end
    -- 尝试获取已有组件
    local bridge = gameObject:GetComponent(CSBridgeType)
    if not bridge then
        -- 不存在则添加新组件
        bridge = gameObject:AddComponent(CSBridgeType)
        CS.UnityEngine.Debug.Log("添加了LuaUIEventBridge组件到: " .. gameObject.name)
    end
    return bridge
end

-- 通用绑定函数
-- gameObject: 绑定事件的UI对象
-- handlerTable: 需要绑定的UI事件处理函数表
function UIEventUtils.Bind(gameObject, handlerTable)
    local bridge = GetOrAddBridge(gameObject)
    if not bridge then return end

    -- 创建Lua表来存储处理函数
    local luaTable

    -- 将处理函数复制到新表中
    for methodName, handler in pairs(handlerTable) do
        luaTable[methodName] = handler
    end
    
    -- 注意：handlerTable需自行管理生命周期，避免循环引用
    bridge.luaTable = luaTable
    CS.UnityEngine.Debug.Log("成功绑定UI事件到: " .. gameObject.name)
end

-- 解除绑定
function UIEventUtils.Unbind(gameObject)
    local bridge = gameObject:GetComponent(CSBridgeType)
    if bridge then
        -- 清除lua引用，避免内存泄漏
        bridge.luaTable = nil
        CS.UnityEngine.Debug.Log("解除绑定: " .. gameObject.name)
    end
end

-- 绑定具体UI事件
-- gameObject: 绑定对象
-- callback: 点击回调函数（参数：self, eventData）
function UIEventUtils.BindClick(gameObject, callback)
    UIEventUtils.Bind(gameObject, {
        OnPointerClick = function(self, eventData)
            CS.UnityEngine.Debug.Log("点击事件触发")
            if callback then
                callback(self, eventData)
            end
        end
    })
end



return UIEventUtils