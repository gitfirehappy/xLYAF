local Log = M.LogUtility

local DialogueModel = {}

-- 初始化对话数据
function DialogueModel.Init(data)
    self.currentDialogue = nil  -- 当前对话数据
    self.currentID = "0"       -- 当前对话ID
    self.dialogueData = data   -- 整段对话配置数据
    self.isEnd = false         -- 对话是否结束
end

-- 获取当前对话数据
function DialogueModel.GetCurrentDialogue()
    for _, dialog in ipairs(self.dialogueData) do
        if dialog["ID"] == self.currentID then
            self.currentDialogue = dialog
            return self.currentDialogue
        end
    end
    return nil
end

-- 检查是否为普通对话类型
function DialogueModel.IsNormalType()
    return self.currentDialogue and self.currentDialogue["sign"] == "#"
end

-- 检查是否为选项类型
function DialogueModel.IsOptionType()
    return self.currentDialogue and self.currentDialogue["sign"] == "&"
end

-- 检查是否为条件判断类型
function DialogueModel.IsConditionType()
    return self.currentDialogue and self.currentDialogue["sign"] == "$"
end

-- 判断执行函数时机
function DialogueModel.CheckExecuteTime()
    if self.currentDialogue then
        
    end
end

-- 清理对话数据
function DialogueModel.Cleanup()
    self.currentDialogue = nil
    self.currentID = nil
    self.dialogueData = nil
    self.isEnd = false
end

return DialogueModel