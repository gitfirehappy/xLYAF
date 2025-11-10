-- 模块类，无需桥接
local PlayerHFSMState = require "PlayerHFSMState"
local FallState = setmetatable({}, {__index = PlayerHFSMState})
FallState.__index = FallState

function FallState.Create(controller)
    local obj = PlayerHFSMState.Create("Fall", controller)
    setmetatable(obj, FallState)
    return obj
end

function FallState:OnEnter()
    if self.anim then
        self.anim:PlayState("Airborne/Fall")
    end
end

function FallState:OnUpdate()
    -- 落地检查在AirborneState中处理
end

function FallState:OnFixedUpdate()
    -- TODO：空中移动可能可以统一放在父状态机（jump，fall）
    -- 空中移动控制（可调整为比地面弱）
    local moveInput = self.inputHandler:GetMoveInput()
    local newVelocityX = moveInput.x * self.controller.playerData.moveSpeed
    local currentVelocity = self.physics:GetVelocity()

    self.physics:ApplyVelocity(CS.UnityEngine.Vector2(newVelocityX, currentVelocity.y))
end

return FallState