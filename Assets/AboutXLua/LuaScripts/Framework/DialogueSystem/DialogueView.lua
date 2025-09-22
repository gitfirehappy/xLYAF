local DialogueView = {}

-- UI组件引用
local dialoguePanel = nil
local optionsParent = nil

-- 显示对话面板
function DialogueView.ShowDialogue()
    if not dialoguePanel then
        -- 通过UIManager显示对话面板
        CS.UIManager.Instance:ShowUIForm("DialoguePanel")
        dialoguePanel = CS.UIManager.Instance:GetForm("DialoguePanel")
    end
    
end

-- 隐藏对话面板
function DialogueView.HideDialogue()
    if dialoguePanel then
        CS.UIManager.Instance:HideUIForm("DialoguePanel")
        dialoguePanel = nil
    end
end

-- 更新对话面板
function DialogueView.UpdateDialogue()
    
end

-- 显示选项
function DialogueView.ShowOptions()
    
end

return DialogueView
