
using XLua;

public static class LuaDelegateConfig
{
    //要求显式声明哪些 C# 委托类型可以调用 Lua 函数
    [CSharpCallLua]
    public delegate int AddDelegate(int a, int b);
}