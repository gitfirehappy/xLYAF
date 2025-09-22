local DialogueModel = {}

-- 初始化对话数据
function DialogueModel:Init(data)
    self.currentDialogue = nil  -- 当前对话数据
    self.currentID = "0"       -- 当前对话ID
    self.dialogueData = data   -- 整段对话配置数据
    self.isEnd = false         -- 对话是否结束
end

-- 获取当前对话数据
function DialogueModel:GetCurrentDialogue()
    for _, dialog in ipairs(self.dialogueData) do
        if dialog.ID == self.currentID then
            self.currentDialogue = dialog
            return self.currentDialogue
        end
    end
    return nil
end

-- 检查是否为普通对话类型
function DialogueModel:IsNormalType()
    return self.currentDialogue and self.currentDialogue.Sign == "#"
end

-- 检查是否为选项类型
function DialogueModel:IsOptionType()
    return self.currentDialogue and self.currentDialogue.Sign == "%"
end

-- 检查是否为条件判断类型
function DialogueModel:IsConditionType()
    return self.currentDialogue and self.currentDialogue.Sign == "$"
end

-- 检查并返回即时执行函数及其入参（>前缀）
function DialogueModel:GetImmediateFunc()
    local func = self.currentDialogue.Func or ""
    if string.sub(func, 1, 1) == ">" then
        return string.sub(func, 2), self:ParseParams()
    end
    return nil, nil
end

-- 检查并返回交互执行函数及其入参（<前缀）
function DialogueModel:GetInteractiveFunc()
    local func = self.currentDialogue.Func or ""
    if string.sub(func, 1, 1) == "<" then
        return string.sub(func, 2), self:ParseParams()
    end
    return nil, nil
end

-- 解析参数（分号分隔转数组）
function DialogueModel:ParseParams()
    return self.currentDialogue.Parameter and string.split(self.currentDialogue.Parameter, ";") or {}
end

-- 更新当前ID（处理END标识）
function DialogueModel:UpdateCurrentID(nextID)
    if nextID == "END" then
        self.isEnd = true
        self.currentID = nil
    else
        self.currentID = nextID
    end
end

-- 清理对话数据
function DialogueModel:Cleanup()
    self.currentDialogue = nil
    self.currentID = nil
    self.dialogueData = nil
    self.isEnd = false
end

return DialogueModel