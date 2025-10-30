using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// LuaBridge类型，不包括LuaBehaviourBridge
/// </summary>
public enum LuaBridgeType
{
    [LuaBridgeTypeMapping(typeof(Collision2DBridge))]
    Collision2D,
    
    [LuaBridgeTypeMapping(typeof(GizmosBridge))]
    Gizmos,
    
    [LuaBridgeTypeMapping(typeof(InputBridge))]
    Input,
    
    [LuaBridgeTypeMapping(typeof(Physics2DBridge))]
    Physics2D,
    
    [LuaBridgeTypeMapping(typeof(UIEventBridge))]
    UIEvent,
    
    [LuaBridgeTypeMapping(typeof(ScriptObjectBridge))]
    SO,
    
    [LuaBridgeTypeMapping(typeof(AnimBridge))]
    Anim,
}
