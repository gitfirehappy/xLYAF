local DialogueView = {}
local stringUtil = require("StringUtil")
local controller = require("DialogueController")
local uiEventUtils = require("UIEventUtils")

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

    -- 绑定普通对话点击面板下一句回调
    if uiRefs.panel then
        uiEventUtils.BindClick(uiRefs.panel.gameObject, function(_, eventData)
            -- 点击对话面板时触发Next
            controller.Next()
        end)
    end
    
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
    if not uiRefs.panel then return end
    
    local characterName = dialogueData.Character or ""
    local content = dialogueData.Content or ""
    local posAndOp = dialogueData.PosAndOp or ""
    
    -- 更新角色名和内容
    -- 第一个角色名为说话人
    -- 分隔好角色名和操作作为List<string>传入CS.DialoguePanel.UpdateCharacter
    local characterNames = stringUtil.SplitSemicolon(characterName)
    local posAndOps = stringUtil.SplitSemicolon(posAndOp)
    
    uiRefs.contentText = content
    uiRefs.characterNameText = characterNames[1]
    
    uiRefs.panel:UpdateCharacter(characterNames, posAndOps)
end

-- 显示选项
function DialogueView.ShowOptions(options, callback)
    if not uiRefs.panel then return end

    -- 提取选项文本
    local optionTexts = {}
    for i, option in ipairs(options) do
        table.insert(optionTexts, option.Content or "")
    end
    
    -- 创建并显示选项
    -- C#端创建时会自动绑定传入的回调
    CS.DialoguePanel.CreateOptions(optionTexts, callback)
end

-- 隐藏选项
function DialogueView.HideOptions()
    if uiRefs.optionsParent then
        -- 清空并隐藏现有选项
        CS.DialoguePanel.ClearOptions()
    end   
end

return DialogueView
