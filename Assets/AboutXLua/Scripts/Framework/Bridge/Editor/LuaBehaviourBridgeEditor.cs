using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LuaBehaviourBridge))]
public class LuaBehaviourBridgeEditor : Editor
{
    private List<LuaBridgeType> selectedBridges = new List<LuaBridgeType>();
    private bool isInitialized = false;
    private bool showBridges = true; // 折叠状态控制变量

    private void OnEnable()
    {
        if (!isInitialized)
        {
            SyncBridgesFromComponent();
            isInitialized = true;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        // 桥接组件区域标题与折叠控制
        EditorGUILayout.LabelField("Bridge Components", EditorStyles.boldLabel);
        showBridges = EditorGUILayout.Foldout(showBridges, "已选桥接组件", true);

        // 折叠状态下显示列表
        if (showBridges)
        {
            EditorGUI.indentLevel++;
            foreach (LuaBridgeType type in Enum.GetValues(typeof(LuaBridgeType)))
            {
                bool isSelected = selectedBridges.Contains(type);
                bool newState = EditorGUILayout.ToggleLeft(type.ToString(), isSelected);

                if (newState != isSelected)
                {
                    if (newState)
                    {
                        AddBridge(type);
                        selectedBridges.Add(type);
                    }
                    else
                    {
                        RemoveBridge(type);
                        selectedBridges.Remove(type);
                    }
                    Repaint();
                }
            }
            EditorGUI.indentLevel--; 
        }
        
        EditorGUILayout.Space();
        if (GUILayout.Button("刷新 Bridge 状态"))
        {
            SyncBridgesFromComponent();
        }
    }
    
    private void SyncBridgesFromComponent()
    {
        selectedBridges.Clear();
        var targetScript = (LuaBehaviourBridge)target;
        var existingBridges = targetScript.GetComponents<IBridge>();
        
        foreach (var bridge in existingBridges)
        {
            if (TryGetBridgeType(bridge.GetType(), out var type))
            {
                selectedBridges.Add(type);
            }
        }
    }

    private void AddBridge(LuaBridgeType type)
    {
        var targetScript = (LuaBehaviourBridge)target;
        Type componentType = GetComponentTypeForBridge(type);
        
        if (componentType != null && targetScript.gameObject.GetComponent(componentType) == null)
        {
            Undo.AddComponent(targetScript.gameObject, componentType);
        }
    }

    private void RemoveBridge(LuaBridgeType type)
    {
        var targetScript = (LuaBehaviourBridge)target;
        Type componentType = GetComponentTypeForBridge(type);
        var component = targetScript.gameObject.GetComponent(componentType);
        
        if (component != null)
        {
            Undo.DestroyObjectImmediate(component);
        }
    }

    private Type GetComponentTypeForBridge(LuaBridgeType type)
    {
        return type switch
        {
            LuaBridgeType.Collision2D => typeof(Collision2DBridge),
            LuaBridgeType.Gizmos => typeof(GizmosBridge),
            LuaBridgeType.Input => typeof(InputBridge),
            LuaBridgeType.Physics2D => typeof(Physics2DBridge),
            LuaBridgeType.UIEvent => typeof(UIEventBridge),
            _ => null
        };
    }

    private bool TryGetBridgeType(Type componentType, out LuaBridgeType bridgeType)
    {
        if (componentType == typeof(Collision2DBridge))
        {
            bridgeType = LuaBridgeType.Collision2D;
            return true;
        }
        if (componentType == typeof(GizmosBridge))
        {
            bridgeType = LuaBridgeType.Gizmos;
            return true;
        }
        if (componentType == typeof(InputBridge))
        {
            bridgeType = LuaBridgeType.Input;
            return true;
        }
        if (componentType == typeof(Physics2DBridge))
        {
            bridgeType = LuaBridgeType.Physics2D;
            return true;
        }
        if (componentType == typeof(UIEventBridge))
        {
            bridgeType = LuaBridgeType.UIEvent;
            return true;
        }
        
        bridgeType = default;
        return false;
    }
}