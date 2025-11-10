-- 模块类，无需桥接
local PlayerHFSMState = require "PlayerHFSMState"
local RunState = setmetatable({}, {__index = PlayerHFSMState})
RunState.__index = RunState

function RunState.Create(controller)
    local obj = PlayerHFSMState.Create("Run", controller)
    setmetatable(obj, RunState)
    return obj
end

function RunState:OnEnter()
    if self.anim then
        self.anim:PlayState("Grounded/Run")
    end
end

function RunState:OnUpdate()
    local moveInput = self.inputHandler:GetMoveInput()
    
    -- 转换到Idle
    if math.abs(moveInput.x) < 0.1 then
        self.stateMachine:ChangeState("Idle")
        return
    end

    -- 转换到跳跃（离地检测在父状态机）
    if self.inputHandler:UseJumpInput() then
        self.stateMachine.parentStateMachine:ChangeState("Airborne")
        return
    end
end

function RunState:OnFixedUpdate()
    local moveInput = self.inputHandler:GetMoveInput()
    local newVelocityX = moveInput.x * self.controller.playerData.moveSpeed
    local currentVelocity = self.physics:GetVelocity()
    
    self.physics:ApplyVelocity(CS.UnityEngine.Vector2(newVelocityX, currentVelocity.y))

    -- 转向处理
    -- TODO:转向应该是整个运动通用逻辑
    if moveInput.x > 0.1 and not self.controller.facingRight then
        self.controller:Flip()
    elseif moveInput.x < -0.1 and self.controller.facingRight then
        self.controller:Flip()
    end
end

return RunState