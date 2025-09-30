local UIEventUtils = {}
local CSBridgeType = CS.LuaUIEventBridge

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
    end
    return bridge
end

-- 通用绑定函数
-- gameObject: 绑定事件的UI对象
-- handlerTable: 需要绑定的UI事件处理函数表
function UIEventUtils.Bind(gameObject, handlerTable)
    local bridge = GetOrAddBridge(gameObject)
    if not bridge then return end

    -- 设置luaTable，事件触发时会调用表中对应方法
    -- 注意：handlerTable需自行管理生命周期，避免循环引用
    bridge.luaTable = handlerTable
end

-- 解除绑定
function UIEventUtils.Unbind(gameObject)
    local bridge = gameObject:GetComponent(CSBridgeType)
    if bridge then
        -- 清除lua引用，避免内存泄漏
        bridge.luaTable = nil
    end
end

-- 绑定具体UI事件
-- gameObject: 绑定对象
-- callback: 点击回调函数（参数：self, eventData）
function UIEventUtils.BindClick(gameObject, callback)
    UIEventUtils.Bind(gameObject, {
        OnPointerClick = callback
    })
end



return UIEventUtils