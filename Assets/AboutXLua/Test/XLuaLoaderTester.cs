using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

public class XLuaLoaderTester : MonoBehaviour
{
    public enum TestMode
    {
        EditorOnly,
        AddressablesOnly,
        Hybrid
    }

    [Header("测试配置")]
    public TestMode testMode = TestMode.Hybrid;
    public string luaModuleName = "HelloWorld";
    
    [Header("XLuaLoader 配置")]
    public List<string> editorRoots = new() { "AboutXLua/Test/LuaScripts" };
    public List<string> aaLabels = new() { "LuaScripts" };
    public bool enableLog = true;
    
    private LuaEnv _luaEnv;
    
    void Start()
    {
        TestXLuaLoader();
    }

    void OnDestroy()
    {
        if (_luaEnv != null)
        {
            _luaEnv.Dispose();
            _luaEnv = null;
        }
    }

    [ContextMenu("测试 XLuaLoader")]
    public void TestXLuaLoader()
    {
        if (_luaEnv != null)
        {
            _luaEnv.Dispose();
        }

        _luaEnv = new LuaEnv();
        
        // 准备测试配置
        var options = new XLuaLoader.Options
        {
            mode = (XLuaLoader.Mode)testMode,
            editorRoots = editorRoots,
            aaLabels = aaLabels,
        };

        // 注册加载器
        XLuaLoader.SetupAndRegister(_luaEnv, options);

        // 执行Lua测试
        try
        {
            _luaEnv.DoString($"require '{luaModuleName}'");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Lua执行错误: {ex.Message}");
        }
    }
}
