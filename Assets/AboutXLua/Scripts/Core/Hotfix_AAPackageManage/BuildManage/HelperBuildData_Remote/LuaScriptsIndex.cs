using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lua 脚本索引
/// 用于记录：某个 LuaScriptContainer (AddressableKey) 包含哪些 Lua 脚本 (NormalizedName)
/// </summary>
[CreateAssetMenu(fileName = Constants.LUA_SCRIPTS_INDEX, menuName = "XLua/扫描导出的 LuaScriptsIndex")]
public class LuaScriptsIndex : ScriptableObject
{
    [Serializable]
    public class ContainerEntry
    {
        public string containerAddress; // AA Key
        public List<string> scriptNames; // 包含的脚本名
    }

    public List<ContainerEntry> data = new();
    
    // 运行时快速查找字典
    private Dictionary<string, List<string>> _containerToScripts;
    private Dictionary<string, string> _scriptToContainer;

    /// <summary>
    /// 构建运行时快速查找字典
    /// </summary>
    public void BuildRuntimeDics()
    {
        _scriptToContainer = new Dictionary<string, string>();
        _containerToScripts = new Dictionary<string, List<string>>();
        
        foreach (var entry in data)
        { 
            _containerToScripts[entry.containerAddress] = entry.scriptNames;

            foreach (var scriptName in entry.scriptNames)
            {
                if (!_scriptToContainer.ContainsKey(scriptName))
                {
                    _scriptToContainer[scriptName] = entry.containerAddress;
                }
                else
                {
                    Debug.LogWarning($"[LuaScriptsIndex] 脚本名冲突: {scriptName} 同时存在于 {_scriptToContainer[scriptName]} 和 {entry.containerAddress}");
                }
            }
        }
    }
}
