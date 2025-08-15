using System.Collections;
using UnityEngine;
using XLua;

public class CoroutineTester : MonoBehaviour
{
    private LuaEnv _luaEnv;
    private bool _isInitialized;
    public string[] customLuaPaths = { "Assets/AboutXLua/LuaScripts/Core/CoroutineScheduler" }; // 自定义路径

    [ContextMenu("1. 初始化环境")]
    public void Init()
    {
        if (_isInitialized) return;
        
        CSharpCoroutineScheduler.Init(this);
        _luaEnv = new LuaEnv();
        
        // 注册自定义加载器
        _luaEnv.AddLoader((ref string path) => {
            foreach (var basePath in customLuaPaths) {
                string fullPath = $"{basePath}/{path.Replace('.', '/')}.lua";
                if (System.IO.File.Exists(fullPath)) {
                    return System.IO.File.ReadAllBytes(fullPath);
                }
            }
            return null;
        });
        
        // 正确加载Lua模块
        _luaEnv.DoString("coroutineBridge = require 'coroutineBridge'");
        
        _isInitialized = true;
        Debug.Log("✅ 环境初始化完成");
    }

    [ContextMenu("2. 测试C#协程")]
    public void TestCSharpCoroutine()
    {
        CheckInit();
        int id = CSharpCoroutineScheduler.StartCoroutine(SimpleCSharpCoroutine(), _luaEnv);
        Debug.Log($"🚀 C#协程启动 ID:{id}");
    }

    private IEnumerator SimpleCSharpCoroutine()
    {
        int id = CSharpCoroutineScheduler.GetCurrentCoroutineId();
        Debug.Log($"⚡ C#{id} 开始");
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"⚡ C#{id} 运行中");
        yield return new WaitForSeconds(0.5f);
        Debug.Log($"✅ C#{id} 完成");
    }

    [ContextMenu("3. 测试Lua协程")]
    public void TestLuaCoroutine()
    {
        CheckInit();
        
        // 正确使用DoString，忽略返回值
        _luaEnv.DoString(@"
            local id = coroutineBridge.create(function()
                print('🌌 Lua#'..coroutineBridge.get_current_id()..' 开始')
                coroutine.yield()
                print('🌌 Lua#'..coroutineBridge.get_current_id()..' 运行中')
                coroutine.yield()
                print('✅ Lua#'..coroutineBridge.get_current_id()..' 完成')
            end)
            print('🚀 Lua协程启动 ID:'..id)
            coroutineBridge.resume(id)
        ", "LuaTest");
    }

    [ContextMenu("4. 测试Lua等待C#")]
    public void TestLuaWaitCSharp()
    {
        CheckInit();
        
        // 正确使用DoString，获取返回值
        object[] result = _luaEnv.DoString(@"
            local csId = coroutineBridge.run_csharp_coroutine(function()
                return CS.UnityEngine.WaitForSeconds(0.5)
            end)
            
            local id = coroutineBridge.create(function()
                print('⏳ Lua开始等待C#'..csId)
                coroutineBridge.wait_for_csharp(csId)
                print('✅ Lua结束等待')
            end)
            
            coroutineBridge.resume(id)
            return id
        ", "LuaWaitTest");
        
        Debug.Log($"🔄 Lua等待C#启动 ID:{result[0]}");
    }

    [ContextMenu("5. 测试C#等待Lua")]
    public void TestCSharpWaitLua()
    {
        CheckInit();
        int id = CSharpCoroutineScheduler.StartCoroutine(WaitForLuaRoutine(), _luaEnv);
        Debug.Log($"🔄 C#等待Lua启动 ID:{id}");
    }

    private IEnumerator WaitForLuaRoutine()
    {
        Debug.Log("⏳ C#开始等待Lua");
        
        // 正确使用DoString获取Lua协程ID
        object[] result = _luaEnv.DoString(@"
            return coroutineBridge.create(function()
                print('🌌 被等待的Lua协程开始')
                coroutine.yield()
                print('🌌 被等待的Lua协程继续')
                coroutine.yield()
                print('✅ 被等待的Lua协程完成')
            end)
        ", "LuaTask");
        
        int luaId = (int)result[0];
        Debug.Log($"📡 创建Lua协程 ID:{luaId}");
        
        // 恢复Lua协程
        _luaEnv.DoString($"coroutineBridge.resume({luaId})", "ResumeLua");
        
        // C#等待Lua协程完成
        yield return CoroutineBridge.WaitForLuaCoroutine(luaId);
        
        Debug.Log("✅ C#结束等待");
    }

    [ContextMenu("6. 清理环境")]
    public void Cleanup()
    {
        if (!_isInitialized) return;
        
        _luaEnv.Dispose();
        _luaEnv = null;
        _isInitialized = false;
        
        Debug.Log("🧹 环境已清理");
    }

    private void CheckInit()
    {
        if (!_isInitialized) 
            throw new System.Exception("❌ 请先初始化环境!");
    }

    private void OnDestroy() => Cleanup();
}