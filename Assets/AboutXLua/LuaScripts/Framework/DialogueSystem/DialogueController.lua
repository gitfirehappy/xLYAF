local DialogueController = {}

local view = require("DialogueView")
local model = require("DialogueModel")
local dataManager = require("DialogueDataManager")
local currentDialogueFile = nil

-- 开始一段对话
function DialogueController.Start(fileName)
    print("Start dialogue: " .. fileName)
    currentDialogueFile = fileName
    
    -- 加载对话数据并初始化Model
    local dialogueDta = dataManager.LoadDialogueData(fileName)
    if dialogueDta == nil then
        CS.Debug.LogError("Load dialogue data failed")
        return
    end
    model:Init(dialogueDta)
    
    -- 更新View
    view.ShowDialogue();
    DialogueController.Refresh()
end

-- 刷新对话（核心流程）
function DialogueController.Refresh()
    if model.isEnd then
        DialogueController.End()
    end

    -- 获取当前对话并检查有效性
    local currentDialog = model:GetCurrentDialogue()
    if not currentDialog then
        DialogueController.End()
        return
    end

    -- 执行即时函数（>前缀，出现时执行）
    local funcName, params = model:GetImmediateFunc()
    if funcName then
        DialogueController.Execute(funcName, params)
    end
    
    -- 根据类型更新视图
    if model:IsNormalType() then
        
    elseif model:IsOptionType() then
        
    elseif model:IsConditionType() then
        
    end
end

-- 下一条对话
function DialogueController.Next()
    
end 

-- 执行函数
function DialogueController.Execute(funcName, params)
    print("Execute: " .. funcName)
    -- xlua 调用 C#函数（C#注册对话函数表）
    -- local csharpFunc = 
    if csharpFunc then
        -- 返回条件判断所需参数
    else
        CS.Debug.LogWarning("Func not found: " .. funcName)        
    end
end

-- 结束这段对话
function DialogueController.End()
    print("End dialogue: " .. (currentDialogueFile or ""))
    view.HideDialogue()
    model:Cleanup()
    -- 可选：卸载数据
    -- dataManager.UnloadDialogueData(currentDialogueFile)
    currentDialogueFile = nil
end 