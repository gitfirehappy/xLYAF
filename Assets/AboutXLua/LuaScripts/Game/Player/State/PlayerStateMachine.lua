-- 模块类，无需桥接
local BaseStateMachine = require "BaseStateMachine"
local PlayerStateMachine = setmetatable({}, {__index = BaseStateMachine})
PlayerStateMachine.__index = PlayerStateMachine

function PlayerStateMachine.Create(controller)
    local obj = BaseStateMachine.Create()
    setmetatable(obj, PlayerStateMachine)

    obj.controller = controller
    obj.animBridge = controller.anim

    return obj
end

function PlayerStateMachine:CanMove()
    return not self:IsInState("Dialogue") and not self:IsInState("Disabled")
end

function PlayerStateMachine:IsGrounded()
    return self.controller.isGrounded
end

return PlayerStateMachine