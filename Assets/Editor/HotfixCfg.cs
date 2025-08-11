// Assets/Editor/HotfixCfg.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XLua;

public static class HotfixCfg
{
    // 静态字段方式
    [Hotfix]
    public static List<Type> by_field = new List<Type>()
    {
        typeof(Person_Lesson5), // 这里加入你的可热更类
        // typeof(OtherClass),   // 其他要热更的类
    };

    // 静态属性方式（可批量匹配）
    [Hotfix]
    public static List<Type> by_property
    {
        get
        {
            // 示例：批量加入命名空间为 "MyGame.Logic" 的类
            return (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                where type.Namespace == "MyGame.Logic"
                select type).ToList();
        }
    }
}