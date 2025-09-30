local DialogueModel = {}
local stringUtil = require("StringUtil")

-- 初始化对话数据
function DialogueModel:Init(data)
    self.currentID = "0"       -- 当前对话ID
    self.dialogueData = data   -- 整段对话配置数据
    self.isEnd = false         -- 对话是否结束
    self.optionIDs = nil       -- 选项ID列表（多个NextID时使用）
end

-- 获取当前对话数据
function DialogueModel:GetCurrentDialogue()
    for _, dialog in ipairs(self.dialogueData) do
        if dialog.ID == self.currentID then
            return dialog
        end
    end
    return nil
end

-- 检查是否为条件判断类型
function DialogueModel:IsConditionType()
    local current = self:GetCurrentDialogue()
    return current and current.Sign == "$"
end

-- 检查是否为普通语句类型
function DialogueModel:IsNormalType()
    local current = self:GetCurrentDialogue()
    return current and current.Sign == "#"
end

-- 获取即时执行函数（>前缀）
function DialogueModel:GetImmediateFunc()
    local current = self:GetCurrentDialogue()
    if not current then return nil, nil end
    local func = current.Func or ""
    if string.sub(func, 1, 1) == ">" then
        return string.sub(func, 2), stringUtil.SplitSemicolon(current.Params or "")
    end
    return nil, nil
end

-- 获取交互执行函数（<前缀）
function DialogueModel:GetInteractiveFunc()
    local current = self:GetCurrentDialogue()
    if not current then return nil, nil end
    local func = current.Func or ""
    if string.sub(func, 1, 1) == "<" then
        return string.sub(func, 2), stringUtil.SplitSemicolon(current.Params or "")
    end
    return nil, nil
end

-- 更新当前ID（处理END和选项ID列表）
function DialogueModel:UpdateCurrentID(nextID)
    self.optionIDs = nil  -- 重置选项ID
    if nextID == "END" then
        self.isEnd = true
        self.currentID = nil
    elseif nextID:find(";") then
        -- 多个ID视为选项
        self.optionIDs = stringUtil.SplitSemicolon(nextID)
    else
        self.currentID = nextID
    end
end

-- 获取选项对应的对话数据列表
function DialogueModel:GetOptions()
    if not self.optionIDs then return {} end
    local options = {}
    for _, id in ipairs(self.optionIDs) do
        for _, dialog in ipairs(self.dialogueData) do
            if dialog.ID == id then
                table.insert(options, dialog)
                break
            end
        end
    end
    return options
end

-- 清理对话数据
function DialogueModel:Cleanup()
    self.currentID = nil
    self.dialogueData = nil
    self.isEnd = false
    self.optionIDs = nil
end

return DialogueModel