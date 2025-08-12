using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// XLua标签批量配置的ScriptableObject容器
/// </summary>
[CreateAssetMenu(fileName = "TypeList", menuName = "XLua/Type List", order = 0)]
public class TypeListSO : ScriptableObject,ISerializationCallbackReceiver
{
    /// <summary>
    /// 需要管理的标签类型
    /// </summary>
    public enum ConfigTag
    {
        Hotfix = 0,
        LuaCallCSharp = 1,
        CSharpCallLua = 2
    }
    [Tooltip("配置分类")]
    public ConfigTag tag;
    
    [Tooltip("类型引用列表")]
    public List<TypeReference> types = new List<TypeReference>();
    
    // 序列化前触发所有TypeReference的序列化处理
    public void OnBeforeSerialize()
    {
        foreach (var typeRef in types)
        {
            typeRef.OnBeforeSerialize();
        }
    }

    // 反序列化后触发所有TypeReference的恢复处理
    public void OnAfterDeserialize()
    {
        foreach (var typeRef in types)
        {
            typeRef.OnAfterDeserialize();
        }
    }
}
