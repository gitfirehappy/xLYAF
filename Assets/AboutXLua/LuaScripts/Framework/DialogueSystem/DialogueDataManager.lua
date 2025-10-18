local DialogueDataManager = {}
local loadedDialogues = {} -- 缓存已加载的对话

-- TODO: 目前为基础单个文件加载，可增加批量加载和格式转换
-- 加载对话数据
function DialogueDataManager.LoadDialogueData(fileName)
    if  loadedDialogues[fileName] then
        return loadedDialogues[fileName]
    end

    -- 加载lua对话文件（每个对话一个文件）
    local ok, data = pcall(require, fileName)
    if ok and data then
        loadedDialogues[fileName] = data
        CS.UnityEngine.Debug.Log("Successfully loaded dialogue: " .. fileName)
        return data
    else
        CS.UnityEngine.Debug.LogError("Load dialogue file failed: " .. fileName .. ", error: " .. tostring(data))
        return nil
    end
end

-- 卸载对话文件
function DialogueDataManager.UnloadDialogue(fileName)
    if loadedDialogues[fileName] then
        loadedDialogues[fileName] = nil
        -- 移除lua模块缓存
        package.loaded[fileName] = nil
    end
end

return DialogueDataManager
