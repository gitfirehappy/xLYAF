using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TypeSelectionWindow : EditorWindow
{
    private string searchFilter = "";
    private Vector2 scrollPosition;
    private SerializedProperty targetProperty;
    private Type[] filteredTypes;
    private static Type[] allTypesCache;

    public static void Open(SerializedProperty property)
    {
        var window = GetWindow<TypeSelectionWindow>("Select Type");
        window.targetProperty = property;
        window.minSize = new Vector2(400, 500);
        CacheAssemblyTypes();
    }

    private static void CacheAssemblyTypes()
    {
        if (allTypesCache != null) return;
        
        allTypesCache = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic && 
                   !asm.FullName.StartsWith("UnityEngine") && 
                   !asm.FullName.StartsWith("UnityEditor"))
            .SelectMany(asm => 
            {
                try { return asm.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t => t.IsPublic && !t.IsAbstract)
            .OrderBy(t => t.FullName)
            .ToArray();
    }

    private void OnGUI()
    {
        if (targetProperty == null) 
        {
            Close();
            return;
        }
        
        try
        {
            // 使用正确的样式名称
            GUIStyle searchFieldStyle = GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.textField;
            GUIStyle cancelButtonStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.miniButton;

            // 搜索栏
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                // 使用安全的样式获取方式
                searchFilter = EditorGUILayout.TextField(searchFilter, searchFieldStyle);
                
                // 修复3: 添加null检查
                if (GUILayout.Button("", cancelButtonStyle))
                {
                    searchFilter = "";
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();

            // 类型列表
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                // 清空选项
                if (GUILayout.Button("None"))
                {
                    SetType(null);
                    Close();
                }

                // 应用搜索过滤
                if (allTypesCache != null)
                {
                    filteredTypes = string.IsNullOrEmpty(searchFilter) 
                        ? allTypesCache.Take(300).ToArray() 
                        : allTypesCache
                            .Where(t => t.FullName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                            .Take(200)
                            .ToArray();

                    // 显示类型
                    foreach (Type type in filteredTypes)
                    {
                        string label = $"{type.Namespace}.{type.Name}";
                        if (GUILayout.Button(label, EditorStyles.label))
                        {
                            SetType(type);
                            Close();
                        }
                    }
                }
                else
                {
                    GUILayout.Label("No types available", EditorStyles.centeredGreyMiniLabel);
                }
            }
            EditorGUILayout.EndScrollView();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Type selection error: {ex.Message}");
            Close();
        }
    }

    private void SetType(Type type)
    {
        if (targetProperty == null) return;
        
        SerializedProperty assemblyProp = targetProperty.FindPropertyRelative("assemblyName");
        SerializedProperty typeProp = targetProperty.FindPropertyRelative("typeName");
        
        if (assemblyProp != null) assemblyProp.stringValue = type?.Assembly.GetName().Name ?? "";
        if (typeProp != null) typeProp.stringValue = type?.FullName ?? "";
        
        targetProperty.serializedObject.ApplyModifiedProperties();
    }
}