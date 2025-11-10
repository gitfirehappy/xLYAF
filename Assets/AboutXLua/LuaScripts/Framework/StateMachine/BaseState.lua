-- 模块类，无需桥接
local BaseState = {}
BaseState.__index = BaseState

function BaseState.Create(name)
    local obj = {}
    setmetatable(obj, BaseState)
    
    obj.Name = name or "UnnamedState"
    obj.stateMachine = nil
    
    return obj
end

-- 以下方法需子类重写

function BaseState:OnEnter(prevState)
    
end

function BaseState:OnUpdate()
    
end

function BaseState:OnFixedUpdate()
    
end

function BaseState:OnExit(nextState)
    
end

return BaseState