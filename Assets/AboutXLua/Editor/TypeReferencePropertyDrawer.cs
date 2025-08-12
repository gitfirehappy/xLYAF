// File: Assets/Editor/TypeReferencePropertyDrawer.cs
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// TypeReference的Inspector绘制器
/// </summary>
[CustomPropertyDrawer(typeof(TypeReference))]
public class TypeReferencePropertyDrawer : PropertyDrawer
{
    private static Type[] s_allAssemblyTypes; // 缓存所有程序集中的类型，供搜索使用
    private static string s_searchFilter = ""; // 搜索字符串

    // 在第一次绘制时加载所有类型
    static TypeReferencePropertyDrawer()
    {
        CacheAllAssemblyTypes();
    }

    private static void CacheAllAssemblyTypes()
    {
        s_allAssemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic 
                          && !asm.Location.EndsWith("mscorlib.dll") 
                          && !asm.Location.Contains("UnityEditor") 
                          && !asm.Location.Contains("com.unity.")) // 过滤掉不相关或动态生成的程序集
            .SelectMany(asm =>
            {
                try { return asm.GetTypes(); } // 尝试获取类型，可能会因为LoadFile异常导致失败，跳过即可
                catch (ReflectionTypeLoadException) { return new Type[0]; }
            })
            .Where(type => type.IsPublic && !type.IsAbstract && !type.IsGenericTypeDefinition) // 筛选公共、非抽象、非泛型定义类型
            .OrderBy(type => type.FullName)
            .ToArray();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 高度为一行的高度
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 使用兼容性更好的布局方式
        Rect controlRect = EditorGUI.PrefixLabel(position, label);
        float spacing = 4f; // 替代standardHorizontalSpacing
    
        // 计算控件尺寸
        float buttonWidth = 60f;
        float typeFieldWidth = controlRect.width - buttonWidth - spacing;
    
        Rect typeRect = new Rect(controlRect.x, controlRect.y, typeFieldWidth, controlRect.height);
        Rect buttonRect = new Rect(controlRect.x + typeFieldWidth + spacing, controlRect.y, buttonWidth, controlRect.height);

        // 显示当前类型状态
        SerializedProperty assemblyProp = property.FindPropertyRelative("assemblyName");
        SerializedProperty typeProp = property.FindPropertyRelative("typeName");
    
        string displayName = "None";
        Color displayColor = Color.gray;
    
        if (!string.IsNullOrEmpty(typeProp.stringValue))
        {
            try
            {
                // 尝试获取类型显示名称
                Assembly asm = Assembly.Load(assemblyProp.stringValue);
                Type type = asm?.GetType(typeProp.stringValue);
                displayName = type != null ? $"{type.Namespace}.{type.Name}" : "Invalid Type!";
                displayColor = type != null ? GUI.color : Color.red;
            }
            catch
            {
                displayName = "Invalid Assembly!";
                displayColor = Color.red;
            }
        }
    
        // 显示类型名称
        EditorGUI.BeginDisabledGroup(true);
        using (new GUIColorScope(displayColor))
        {
            EditorGUI.TextField(typeRect, displayName);
        }
        EditorGUI.EndDisabledGroup();

        // 选择按钮
        if (GUI.Button(buttonRect, "Select"))
        {
            TypeSelectionWindow.Open(property);
        }
    }
    private class GUIColorScope : GUI.Scope
    {
        private readonly Color originalColor;
    
        public GUIColorScope(Color newColor)
        {
            originalColor = GUI.color;
            GUI.color = newColor;
        }
    
        protected override void CloseScope()
        {
            GUI.color = originalColor;
        }
    }
}
