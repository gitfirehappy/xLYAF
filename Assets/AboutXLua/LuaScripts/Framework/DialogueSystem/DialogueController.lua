local Log = M.LogUtility

local DialogueController = {}

local view = require("DialogueView")
local model = require("DialogueModel")
local dataManager = require("DialogueDataManager")
local currentDialogueFile = nil

-- 开始一段对话
function DialogueController.Start(fileName)
    Log.Info(Log.LogLayer.Framework, "DialogueController", "Start dialogue: "..fileName)
    currentDialogueFile = fileName
    
    -- 加载对话数据
    
    -- 初始化Model和 View
    
end

-- 下一条对话
function DialogueController.Next()
    
end 

-- 执行函数
function DialogueController.Execute(funcName, param)
    
end

-- 结束这段对话
function DialogueController.End()
    
end 