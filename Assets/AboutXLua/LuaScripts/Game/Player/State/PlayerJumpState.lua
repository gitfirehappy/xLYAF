-- 模块类，无需桥接
local PlayerHFSMState = require "PlayerHFSMState"
local JumpState = setmetatable({}, {__index = PlayerHFSMState})
JumpState.__index = JumpState

function JumpState.Create(controller)
    local obj = PlayerHFSMState.Create("Jump", controller)
    setmetatable(obj, JumpState)
    return obj
end

function JumpState:OnEnter()
    if self.anim then
        self.anim:PlayState("Airborne/Jump")
    end

    -- 应用跳跃力
    self.physics:ApplyImpulse(CS.UnityEngine.Vector2.up, self.controller.playerData.jumpForce)
    CS.UnityEngine.Debug.Log("[PlayerJumpState] Jump!")
end

function JumpState:OnUpdate()
    local velocityY = self.physics:GetVelocity().y
    
    -- 检查是否开始下落
    if velocityY <= 0 then
        self.stateMachine:ChangeState("Fall")
        return
    end
end

function JumpState:OnFixedUpdate()
    -- TODO：空中移动可能可以统一放在父状态机（jump，fall）
    -- 空中移动控制（可调整为比地面弱）
    local moveInput = self.inputHandler:GetMoveInput()
    local newVelocityX = moveInput.x * self.controller.playerData.moveSpeed
    local currentVelocity = self.physics:GetVelocity()

    self.physics:ApplyVelocity(CS.UnityEngine.Vector2(newVelocityX, currentVelocity.y))
end

return JumpState