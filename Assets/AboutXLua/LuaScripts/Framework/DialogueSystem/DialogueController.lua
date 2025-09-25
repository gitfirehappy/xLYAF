local DialogueController = {}

local view = require("DialogueView")
local model = require("DialogueModel")
local dataManager = require("DialogueDataManager")
local currentDialogueFile = nil

-- 开始对话
function DialogueController.Start(fileName)
    print("Start dialogue: " .. fileName)
    currentDialogueFile = fileName

    local dialogueData = dataManager.LoadDialogueData(fileName)
    if not dialogueData then
        CS.Debug.LogError("Load dialogue data failed: " .. fileName)
        return
    end

    model:Init(dialogueData)
    view.ShowDialogue()
    DialogueController.Refresh()
end

-- 下一条对话（处理当前对话的交互逻辑）
function DialogueController.Next()
    local currentDialogue = model:GetCurrentDialogue()
    if not currentDialogue then return end

    -- 执行点击后函数（<前缀）
    local funcName, params = model:GetInteractiveFunc()
    if funcName then
        DialogueController.Execute(funcName, params)
    end

    -- 更新当前ID（自动处理选项ID列表）
    model:UpdateCurrentID(currentDialogue.NextID or "END")
    DialogueController.Refresh()
end

-- 选项选中处理
function DialogueController.OnOptionSelect(optionID)
    model:UpdateCurrentID(optionID)  -- 直接跳转至选项对应的ID
    DialogueController.Refresh()
end

-- 刷新对话（核心流程）
function DialogueController.Refresh()
    if model.isEnd then
        DialogueController.End()
        return
    end
    
    view.HideOptions()

    local currentDialogue = model:GetCurrentDialogue()
    if not currentDialogue then
        DialogueController.End()
        return
    end

    -- 执行即时函数（>前缀）
    local funcName, params = model:GetImmediateFunc()
    if funcName then
        local result = DialogueController.Execute(funcName, params)
        
        -- 处理条件判断返回值（仅条件类型需要）
        if model:IsConditionType() and result then
            model:UpdateCurrentID(tostring(result))
            DialogueController.Refresh()
        end
    end

    -- 显示选项或普通对话
    local options = model:GetOptions()
    if #options > 0 then
        view.ShowOptions(options, DialogueController.OnOptionSelect)
    else
        view.UpdateDialogue(currentDialogue)
    end
end

-- 执行注册的函数
function DialogueController.Execute(funcName, params)
    print("Execute function: " .. funcName)
    return CS.DialogueFuncRegistry.InvokeFunction(funcName, params)
end

-- 结束对话
function DialogueController.End()
    print("End dialogue: " .. (currentDialogueFile or ""))
    view.HideOptions()
    view.HideDialogue()
    model:Cleanup()
    currentDialogueFile = nil
    -- 可选：卸载文件
end

return DialogueController