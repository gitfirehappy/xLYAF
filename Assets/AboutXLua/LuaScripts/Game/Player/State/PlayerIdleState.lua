-- 模块类，无需桥接
local PlayerHFSMState = require "PlayerHFSMState"
local IdleState = setmetatable({}, {__index = PlayerHFSMState})
IdleState.__index = IdleState

function IdleState.Create(controller)
    local obj = PlayerHFSMState.Create("Idle", controller)
    setmetatable(obj, IdleState)
    return obj
end

function IdleState:OnEnter()
    if self.anim then
        self.anim:PlayState("Grounded/Idle")
    end
end

function IdleState:OnUpdate()
    local moveInput = self.inputHandler:GetMoveInput()
    
    -- 转换到跑
    if math.abs(moveInput.x) > 0.1 then
        self.stateMachine:ChangeState("Run")
        return
    end

    -- 转换到跳跃（离地检测在父状态机）
    if self.inputHandler:UseJumpInput() then
        self.stateMachine.parentStateMachine:ChangeState("Airborne")
        return
    end
end

function IdleState:OnFixedUpdate()
   
end

return IdleState
