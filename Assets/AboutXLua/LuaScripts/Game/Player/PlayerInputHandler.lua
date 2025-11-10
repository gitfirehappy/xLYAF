-- 模块类,不需要桥接挂载（可选），统一管理Player输入
local PlayerInputHandler = {}
PlayerInputHandler.__index = PlayerInputHandler

function PlayerInputHandler.Create(inputBridge)
    local obj = {}
    setmetatable(obj, PlayerInputHandler)

    if not inputBridge then
        CS.UnityEngine.Debug.LogError("[PlayerInputHandler] 传入的 InputBridge 为空！")
        return nil
    end

    obj.inputBridge = inputBridge

    -- 输入状态的存储
    obj.MoveInput = CS.UnityEngine.Vector2.zero
    obj.JumpPressed = false
    -- (未来可扩展: obj.DashPressed = false, obj.InteractPressed = false)

    -- 绑定事件
    obj:BindActions()

    return obj
end

-- 绑定所有“事件”类型的输入（如按下、松开）
function PlayerInputHandler:BindActions()
    -- 将 Player/Jump 动作的 "started" 阶段绑定到 self:OnJump 方法
    self.inputBridge:BindAction("Player/Jump", "started", function(self) self:OnJump() end)
end

-- "Jump" 事件的回调
function PlayerInputHandler:OnJump()
    -- 只设置标志位，等待 Controller 或 State 在 Update/FixedUpdate 中消耗
    self.JumpPressed = true
    CS.UnityEngine.Debug.Log("[PlayerInputHandler] Jump Input Received")
end

-- 由 PlayerController 在 Update() 中调用
function PlayerInputHandler:ProcessUpdate()
    
end

-- 由 PlayerController 在 FixedUpdate() 中调用
function PlayerInputHandler:ProcessFixedUpdate()
    self.MoveInput = self.inputBridge:GetVector2("Player/Move")
end

-- 由 PlayerController 在 LateUpdate() 中调用
function PlayerInputHandler:ProcessLateUpdate()
    
end

--
-- 公共 API (供 PlayerController 和 FSM 状态调用)
--

-- 获取移动输入
function PlayerInputHandler:GetMoveInput()
    return self.MoveInput
end

-- 检查并“消耗”跳跃输入
-- 状态机应该在 Update/FixedUpdate 中调用这个
function PlayerInputHandler:UseJumpInput()
    if self.JumpPressed then
        self.JumpPressed = false -- 消耗掉
        return true
    end
    return false
end

return PlayerInputHandler
