using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

/// <summary>
/// XLua类型配置测试器
/// </summary>
public class XLuaConfigTester : MonoBehaviour
{
    [Header("测试配置")]
    [Tooltip("是否在Awake中自动运行测试")]
    public bool runTestOnAwake = true;
    
    [Header("配置参数")]
    [Tooltip("Addressables标签名")]
    public string configLabel = XluaTypeConfigLoader.DefaultConfigLabel;
    
    [Header("类型配置测试结果")]
    [ReadOnly] public int hotfixTypeCount;
    [ReadOnly] public int luaCallCSharpTypeCount;
    [ReadOnly] public int csharpCallLuaTypeCount;
    
    [Header("成员配置测试结果")]
    [ReadOnly] public int hotfixMemberCount;
    [ReadOnly] public int luaCallCSharpMemberCount;
    [ReadOnly] public int csharpCallLuaMemberCount;
    
    [ReadOnly] public string testStatus = "Not Run";
    
    private void Awake()
    {
        if (runTestOnAwake)
        {
            RunTest();
        }
    }
    
    /// <summary>
    /// 执行XLua配置测试
    /// </summary>
    [ContextMenu("Run XLua Config Test")]
    public void RunTest()
    {
        Debug.Log("=== 开始XLua配置测试 ===");
        
        // 初始化加载器
        XluaTypeConfigLoader.Init(configLabel);
        
        // 获取类型统计
        hotfixTypeCount = XluaTypeConfigLoader.HotfixTypes?.Count ?? 0;
        luaCallCSharpTypeCount = XluaTypeConfigLoader.LuaCallCSharpTypes?.Count ?? 0;
        csharpCallLuaTypeCount = XluaTypeConfigLoader.CSharpCallLuaTypes?.Count ?? 0;
        
        // 获取成员统计
        hotfixMemberCount = XluaTypeConfigLoader.HotfixMembers?.Count ?? 0;
        luaCallCSharpMemberCount = XluaTypeConfigLoader.LuaCallCSharpMembers?.Count ?? 0;
        csharpCallLuaMemberCount = XluaTypeConfigLoader.CSharpCallLuaMembers?.Count ?? 0;
        
        // 验证基本功能
        bool hasConfigs = hotfixTypeCount > 0 || 
                         luaCallCSharpTypeCount > 0 || 
                         csharpCallLuaTypeCount > 0 ||
                         hotfixMemberCount > 0 ||
                         luaCallCSharpMemberCount > 0 ||
                         csharpCallLuaMemberCount > 0;
        
        // 类型验证
        bool hotfixTypeValid = ValidateTagTypes(TypeMemberListSO.ConfigTag.Hotfix, XluaTypeConfigLoader.HotfixTypes);
        bool luaCallTypeValid = ValidateTagTypes(TypeMemberListSO.ConfigTag.LuaCallCSharp, XluaTypeConfigLoader.LuaCallCSharpTypes);
        bool csharpCallTypeValid = ValidateTagTypes(TypeMemberListSO.ConfigTag.CSharpCallLua, XluaTypeConfigLoader.CSharpCallLuaTypes);
        
        // 成员验证
        bool hotfixMemberValid = ValidateTagMembers(TypeMemberListSO.ConfigTag.Hotfix, XluaTypeConfigLoader.HotfixMembers);
        bool luaCallMemberValid = ValidateTagMembers(TypeMemberListSO.ConfigTag.LuaCallCSharp, XluaTypeConfigLoader.LuaCallCSharpMembers);
        bool csharpCallMemberValid = ValidateTagMembers(TypeMemberListSO.ConfigTag.CSharpCallLua, XluaTypeConfigLoader.CSharpCallLuaMembers);
        
        // 生成测试报告
        if (hasConfigs && 
            hotfixTypeValid && luaCallTypeValid && csharpCallTypeValid &&
            hotfixMemberValid && luaCallMemberValid && csharpCallMemberValid)
        {
            testStatus = "✅ 测试通过";
            Debug.Log(testStatus);
        }
        else
        {
            testStatus = "❌ 测试失败";
            Debug.LogError(testStatus);
        }
        
        // 打印详细报告
        PrintTestReport();
        Debug.Log("=== XLua配置测试完成 ===");
    }
    
    /// <summary>
    /// 验证指定标签的类型列表
    /// </summary>
    private bool ValidateTagTypes(TypeMemberListSO.ConfigTag tag, List<Type> types)
    {
        if (types == null)
        {
            Debug.LogError($"❌ {tag} 类型列表未初始化");
            return false;
        }
        
        // 检查类型有效性
        int invalidTypes = types.Count(t => t == null);
        if (invalidTypes > 0)
        {
            Debug.LogError($"❌ {tag} 列表包含 {invalidTypes} 个无效类型");
            return false;
        }
        
        Debug.Log($"✅ {tag} 类型列表验证通过 ({types.Count} 个有效类型)");
        return true;
    }
    
    /// <summary>
    /// 验证指定标签的成员列表
    /// </summary>
    private bool ValidateTagMembers(TypeMemberListSO.ConfigTag tag, List<MemberInfo> members)
    {
        if (members == null)
        {
            Debug.LogError($"❌ {tag} 成员列表未初始化");
            return false;
        }
        
        // 检查成员有效性
        int invalidMembers = members.Count(m => m == null);
        if (invalidMembers > 0)
        {
            Debug.LogError($"❌ {tag} 成员列表包含 {invalidMembers} 个无效成员");
            return false;
        }
        
        Debug.Log($"✅ {tag} 成员列表验证通过 ({members.Count} 个有效成员)");
        return true;
    }
    
    /// <summary>
    /// 打印测试报告
    /// </summary>
    private void PrintTestReport()
    {
        Debug.Log($"\n[XLua 配置测试报告]");
        
        // 类型统计
        Debug.Log($"\n[类型配置统计]");
        Debug.Log($"- Hotfix 类型数量: {hotfixTypeCount}");
        Debug.Log($"- Lua调用C# 类型数量: {luaCallCSharpTypeCount}");
        Debug.Log($"- C#调用Lua 类型数量: {csharpCallLuaTypeCount}");
        
        // 成员统计
        Debug.Log($"\n[成员配置统计]");
        Debug.Log($"- Hotfix 成员数量: {hotfixMemberCount}");
        Debug.Log($"- Lua调用C# 成员数量: {luaCallCSharpMemberCount}");
        Debug.Log($"- C#调用Lua 成员数量: {csharpCallLuaMemberCount}");
        
        // 打印示例
        PrintTypeExamples("Hotfix 示例类型", XluaTypeConfigLoader.HotfixTypes);
        PrintTypeExamples("LuaCallCSharp 示例类型", XluaTypeConfigLoader.LuaCallCSharpTypes);
        PrintTypeExamples("CSharpCallLua 示例类型", XluaTypeConfigLoader.CSharpCallLuaTypes);
        
        PrintMemberExamples("Hotfix 示例成员", XluaTypeConfigLoader.HotfixMembers);
        PrintMemberExamples("LuaCallCSharp 示例成员", XluaTypeConfigLoader.LuaCallCSharpMembers);
        PrintMemberExamples("CSharpCallLua 示例成员", XluaTypeConfigLoader.CSharpCallLuaMembers);
    }
    
    /// <summary>
    /// 打印类型示例
    /// </summary>
    private void PrintTypeExamples(string header, List<Type> types)
    {
        if (types == null || types.Count == 0) 
        {
            Debug.Log($"\n{header}: 无配置");
            return;
        }
        
        Debug.Log($"\n{header}:");
        for (int i = 0; i < Mathf.Min(3, types.Count); i++)
        {
            Debug.Log($"  {types[i].FullName}");
        }
        
        if (types.Count > 3)
        {
            Debug.Log($"  ... 以及 {types.Count - 3} 个其他类型");
        }
    }
    
    /// <summary>
    /// 打印成员示例
    /// </summary>
    private void PrintMemberExamples(string header, List<MemberInfo> members)
    {
        if (members == null || members.Count == 0) 
        {
            Debug.Log($"\n{header}: 无配置");
            return;
        }
        
        Debug.Log($"\n{header}:");
        for (int i = 0; i < Mathf.Min(3, members.Count); i++)
        {
            var member = members[i];
            string memberInfo = GetMemberInfoString(member);
            Debug.Log($"  {memberInfo}");
        }
        
        if (members.Count > 3)
        {
            Debug.Log($"  ... 以及 {members.Count - 3} 个其他成员");
        }
    }
    
    /// <summary>
    /// 获取成员信息的字符串表示
    /// </summary>
    private string GetMemberInfoString(MemberInfo member)
    {
        if (member == null) return "无效成员";
        
        string memberType = member.MemberType.ToString();
        string declaringType = member.DeclaringType?.FullName ?? "未知类型";
        
        if (member is MethodInfo method)
        {
            string parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
            return $"{declaringType}.{method.Name}({parameters}) - {memberType}";
        }
        else if (member is PropertyInfo property)
        {
            return $"{declaringType}.{property.Name} : {property.PropertyType.Name} - {memberType}";
        }
        else if (member is FieldInfo field)
        {
            return $"{declaringType}.{field.Name} : {field.FieldType.Name} - {memberType}";
        }
        
        return $"{declaringType}.{member.Name} - {memberType}";
    }
}

/// <summary>
/// 只读属性绘制器（用于Inspector显示）
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
