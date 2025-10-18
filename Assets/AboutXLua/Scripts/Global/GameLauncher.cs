using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    [Header("XLua Configuration")]
    [Tooltip("Addressables标签名，用于加载XLua类型配置")]
    public string xluaConfigLabel = XluaTypeConfigLoader.DefaultConfigLabel;
    
    [Header("Lua Loader Configuration")]
    [Tooltip("Lua加载模式")]
    public XLuaLoader.Mode loaderMode = XLuaLoader.Mode.Hybrid;
    
    [Tooltip("编辑器模式下的Lua脚本根目录")]
    public List<string> editorRoots = new() { "LuaScripts" };
    
    [Tooltip("Addressables标签，用于加载Lua脚本")]
    public List<string> aaLabels = new() { "LuaScripts" };
    
    async void Awake()
    {
        await BootPhase();
        await InitPhase();
        await StartPhase();
        
        LaunchSignal.NotifyLaunched();
        Debug.Log("=== GameLauncher: All System ready ===");
    }

    private async Task BootPhase()
    {
        Debug.Log("=== Boot Phase ===");
        
        XluaTypeConfigLoader.Init(xluaConfigLabel);
        
        LuaEnvManager.CreateNewEnv();
        
        var loaderOptions = new XLuaLoader.Options
        {
            mode = loaderMode,
            editorRoots = editorRoots,
            aaLabels = aaLabels,
        };
        XLuaLoader.SetupAndRegister(LuaEnvManager.Get(), loaderOptions);

        await Task.CompletedTask;
    }
    
    private async Task InitPhase()
    {
        Debug.Log("=== Init Phase ===");
        LuaModuleRegistry.Initialize();

        await Task.CompletedTask;
    }
    
    private async Task StartPhase()
    {
        Debug.Log("=== Start Phase ===");
        
        DialogueFuncRegistry.ScanAndRegister();
        
        GameUIManager.Instance.Initialize();
        
        await Task.CompletedTask;
    }
}
