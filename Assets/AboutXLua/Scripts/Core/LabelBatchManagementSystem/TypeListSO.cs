using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TypeList", menuName = "XLua/Type List", order = 0)]
public class TypeListSO : ScriptableObject
{
    public enum ConfigTag
    {
        Hotfix = 0,
        LuaCallCSharp = 1,
        CSharpCallLua = 2
    }

    public ConfigTag tag;
    public List<TypeReference> types = new List<TypeReference>();
}
