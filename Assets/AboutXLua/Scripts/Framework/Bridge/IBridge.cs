using XLua;

public interface IBridge
{
    // 初始化桥接组件，传入Lua实例
    void Initialize(LuaTable luaInstance);
}