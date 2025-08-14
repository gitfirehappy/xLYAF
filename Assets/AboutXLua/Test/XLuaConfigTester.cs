using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
    
    [Header("测试结果")]
    [ReadOnly] public int hotfixTypeCount;
    [ReadOnly] public int luaCallCSharpTypeCount;
    [ReadOnly] public int csharpCallLuaTypeCount;
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
        
        // 验证基本功能
        bool hasConfigs = hotfixTypeCount > 0 || 
                         luaCallCSharpTypeCount > 0 || 
                         csharpCallLuaTypeCount > 0;
        
        // 类型验证
        bool hotfixValid = ValidateTagTypes(TypeListSO.ConfigTag.Hotfix, XluaTypeConfigLoader.HotfixTypes);
        bool luaCallValid = ValidateTagTypes(TypeListSO.ConfigTag.LuaCallCSharp, XluaTypeConfigLoader.LuaCallCSharpTypes);
        bool csharpCallValid = ValidateTagTypes(TypeListSO.ConfigTag.CSharpCallLua, XluaTypeConfigLoader.CSharpCallLuaTypes);
        
        // 生成测试报告
        if (hasConfigs && hotfixValid && luaCallValid && csharpCallValid)
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
    private bool ValidateTagTypes(TypeListSO.ConfigTag tag, List<Type> types)
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
        
        Debug.Log($"✅ {tag} 列表验证通过 ({types.Count} 个有效类型)");
        return true;
    }
    
    /// <summary>
    /// 打印测试报告
    /// </summary>
    private void PrintTestReport()
    {
        Debug.Log($"\n[XLua 配置测试报告]");
        Debug.Log($"- Hotfix 类型数量: {hotfixTypeCount}");
        Debug.Log($"- Lua调用C# 类型数量: {luaCallCSharpTypeCount}");
        Debug.Log($"- C#调用Lua 类型数量: {csharpCallLuaTypeCount}");
        
        PrintTypeExamples("Hotfix 示例类型", XluaTypeConfigLoader.HotfixTypes);
        PrintTypeExamples("LuaCallCSharp 示例类型", XluaTypeConfigLoader.LuaCallCSharpTypes);
        PrintTypeExamples("CSharpCallLua 示例类型", XluaTypeConfigLoader.CSharpCallLuaTypes);
    }
    
    /// <summary>
    /// 打印类型示例
    /// </summary>
    private void PrintTypeExamples(string header, List<Type> types)
    {
        if (types == null || types.Count == 0) return;
        
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