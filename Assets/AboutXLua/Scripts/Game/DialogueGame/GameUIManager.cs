using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

public class GameUIManager : SingletonMono<GameUIManager>
{
    public UIResourceConfigSO uiResourceConfig; // TODO: 此处需要替换为AAPackageManager的获取

    private string firstDialogueFileName;
    
    private LuaEnv _luaEnv;
    
    protected override async void Init()
    {
        await LaunchSignal.WaitForLaunch();
        
        _luaEnv = LuaEnvManager.Get();
        
        UIManager.Instance.ShowUIForm<DialoguePanel>();
        
        // LuaEnvManager获取Env开启第一段对话
         _luaEnv.DoString($@"
             local DialogueController = require('DialogueController')
             DialogueController.Start('{firstDialogueFileName}')
         ");
        
        Debug.Log("=== GameUIManager: Init ===");
    }

    public void Initialize()
    {
        UIManager.Instance.Initialize(uiResourceConfig);
    }
}
