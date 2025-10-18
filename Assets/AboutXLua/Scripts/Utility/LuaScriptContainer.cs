using System.Collections.Generic;
using UnityEngine;

namespace AboutXLua.Utility
{
    [CreateAssetMenu(fileName = "LuaContainer", menuName = "XLua/Lua Script Container", order = 2)]
    public class LuaScriptContainer : ScriptableObject
    {
        public string groupName;
        [Tooltip("多个标签可用于细分加载逻辑")]
        public List<string> addressableLabels = new List<string> { "LuaScripts" };
        public List<TextAsset> luaAssets = new List<TextAsset>();

        [ContextMenu("清空列表")]
        public void ClearList()
        {
            luaAssets.Clear();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}