-- 模块类，无需桥接
local PlayerHFSMState = require "PlayerHFSMState"
local AirborneState = setmetatable({}, {__index = PlayerHFSMState})
AirborneState.__index = AirborneState

function AirborneState.Create(controller)
    local obj = PlayerHFSMState.Create("Airborne", controller)
    setmetatable(obj, AirborneState)

    obj:InitializeSubStateMachine()
    return obj
end

function AirborneState:InitializeSubStateMachine()
    local BaseStateMachine = require "BaseStateMachine"
    self.subStateMachine = BaseStateMachine.Create()

    -- 添加空中子状态
    local PlayerJumpState = require "PlayerJumpState"
    local PlayerFallState = require "PlayerFallState"

    self.subStateMachine:AddState("Jump", PlayerJumpState.Create(self.controller))
    self.subStateMachine:AddState("Fall", PlayerFallState.Create(self.controller))
end

function AirborneState:GetInitialSubState()
    local velocityY = self.physics:GetVelocity().y
    return velocityY > 0 and "Jump" or "Fall"
end

function AirborneState:OnEnter(prevState)
    PlayerHFSMState.OnEnter(self, prevState) 

    -- 设置初始子状态
    local initialState = self:GetInitialSubState()
    if initialState and self.subStateMachine then
        self.subStateMachine:ChangeState(initialState)
    end
end

function AirborneState:OnUpdate()
    -- 先处理子状态机
    PlayerHFSMState.OnUpdate(self)

    -- 检查是否落地
    if self.controller.isGrounded then
        self.stateMachine:ChangeState("Grounded")
        return
    end
end

return AirborneState