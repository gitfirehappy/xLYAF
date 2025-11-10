-- 模块类，无需桥接
local PlayerHFSMState = require "PlayerHFSMState"
local GroundedState = setmetatable({}, {__index = PlayerHFSMState})
GroundedState.__index = GroundedState

function GroundedState.Create(controller)
    local obj = PlayerHFSMState.Create("Grounded", controller)
    setmetatable(obj, GroundedState)

    obj:InitializeSubStateMachine()
    return obj
end

function GroundedState:InitializeSubStateMachine()
    local BaseStateMachine = require "BaseStateMachine"
    self.subStateMachine = BaseStateMachine.Create()

    -- 添加地面子状态
    local PlayerIdleState = require "PlayerIdleState"
    local PlayerRunState = require "PlayerRunState"

    self.subStateMachine:AddState("Idle", PlayerIdleState.Create(self.controller))
    self.subStateMachine:AddState("Run", PlayerRunState.Create(self.controller))
end

function GroundedState:GetInitialSubState()
    return "Idle"
end

function GroundedState:OnEnter(prevState)
    PlayerHFSMState.OnEnter(self)

    -- 设置初始子状态
    local initialState = self:GetInitialSubState()
    if initialState and self.subStateMachine then
        self.subStateMachine:ChangeState(initialState)
    end
end

function GroundedState:OnUpdate()
    -- 先处理子状态机
    PlayerHFSMState.OnUpdate(self)

    -- 检查是否离开地面
    if not self.controller.isGrounded then
        self.stateMachine:ChangeState("Airborne")
        return
    end
end

return GroundedState