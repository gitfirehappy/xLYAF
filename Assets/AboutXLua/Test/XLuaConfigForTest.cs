using System.Collections;
using XLua;

public static class XLuaConfig
{
    // 告诉 XLua 需要为 IEnumerator 生成适配代码
    [CSharpCallLua]
    public static System.Collections.Generic.List<System.Type> CSharpCallLua = new System.Collections.Generic.List<System.Type>()
    {
        typeof(IEnumerator),
        typeof(System.Func<System.Collections.IEnumerator>)
    };
}