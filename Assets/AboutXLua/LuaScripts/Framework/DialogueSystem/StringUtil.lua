local StringUtil = {}

-- 通用分号分隔字符串解析（去除空格）
function StringUtil.SplitSemicolon(str)
    if not str or str == "" then return {} end

    local result = {}
    for param in string.gmatch(str, "([^;]+)") do
        table.insert(result, string.match(param, "^%s*(.-)%s*$"))
    end
    return result
end

return StringUtil
