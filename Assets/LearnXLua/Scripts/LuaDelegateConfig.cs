using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using XLua;

// 必须添加[CSharpCallLua]特性
[CSharpCallLua]
public static class LuaDelegateConfig
{
    // 声明所有需要与Lua交互的委托类型
    [CSharpCallLua]
    public delegate void Action_TMP_Text_string(TMP_Text text, string str);
    
    [CSharpCallLua]
    public delegate void Action_TMP_Text(TMP_Text text);
    
    [CSharpCallLua]
    public delegate void Action_Image_string(Image img, string spriteName);
    
    [CSharpCallLua]
    public delegate void Action_TMP_Text_int(TMP_Text text, int num);
    
    // 必须添加这个静态字段以支持代码生成
    [CSharpCallLua]
    public static List<Type> csharpCallLuaTypes = new List<Type>
    {
        typeof(Action_TMP_Text_string),
        typeof(Action_TMP_Text),
        typeof(Action_Image_string),
        typeof(Action_TMP_Text_int)
    };
}