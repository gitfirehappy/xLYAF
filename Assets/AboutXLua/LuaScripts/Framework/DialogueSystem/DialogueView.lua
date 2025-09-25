local DialogueView = {}

-- UI组件引用
local uiRefs = {
    panel = nil,
    optionsParent = nil,
    characterNameText = nil,
    contentText = nil,
    characterImages = {} -- 角色图片引用
}

local function InitUI()
    if uiRefs.panel then return end
    uiRefs.panel = CS.UIManager.Instance:GetUIForm("DialoguePanel")

    -- 绑定UI组件
    uiRefs.optionsParent = uiRefs.panel.optionsParent
    uiRefs.contentText = uiRefs.panel.contentText
    uiRefs.characterNameText = uiRefs.panel.characterNameText

    DialogueView.HideOptions()
end

-- 显示对话面板
function DialogueView.ShowDialogue()
    InitUI()
    CS.UIManager.Instance:ShowUIForm("DialoguePanel")
end

-- 隐藏对话面板
function DialogueView.HideDialogue()
    if uiRefs.panel then
        CS.UIManager.Instance:HideUIForm("DialoguePanel")
        uiRefs.panel = nil
        uiRefs.optionsParent = nil
        uiRefs.contentText = nil
        uiRefs.characterNameText = nil
        uiRefs.characterImages = {}
    end
end

-- 更新对话
function DialogueView.UpdateDialogue(dialogueData)
    InitUI()

    -- 更新角色名和内容
    uiRefs.characterNameText.text = dialogueData.Character or ""
    uiRefs.contentText.text = dialogueData.Content or ""
    
    -- 处理角色位置和显示
    
end

-- 显示选项
function DialogueView.ShowOptions(dialogueData)
    InitUI()
    uiRefs.optionsParent:SetActive(true)
    -- 清空现有选项
    CS.UnityEngine.Object.DestroyChildren(uiRefs.optionsParent)
    
    -- 创建选项
    CS.DialoguePanel.CreateOptions()
    
end

-- 隐藏选项
function DialogueView.HideOptions()
    if uiRefs.optionsParent then
        uiRefs.optionsParent:SetActive(false)
    end   
end

return DialogueView
