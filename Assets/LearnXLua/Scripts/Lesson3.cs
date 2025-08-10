using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lesson3 : MonoBehaviour
{
    public LuaInit luaInit;

    [Header("UI Reference")] 
    public TMP_Text helloText;
    public Button clilckButton;
    public Image targetImage;
    public TMP_Text timerText;

    private LuaDelegateConfig.Action_TMP_Text_string setText;
    private LuaDelegateConfig.Action_TMP_Text onButtonClick;
    private LuaDelegateConfig.Action_Image_string changeImage;
    private LuaDelegateConfig.Action_TMP_Text_int updateTimer;

    public int timeLeft = 10;
    
    void Start()
    {
        //加载Lua脚本
        luaInit.luaEnv.DoString("require 'Lesson3_ui'");
        
        //获取Lua脚本中的函数
        setText = luaInit.luaEnv.Global.Get<LuaDelegateConfig.Action_TMP_Text_string>("set_text");
        onButtonClick = luaInit.luaEnv.Global.Get<LuaDelegateConfig.Action_TMP_Text>("on_button_click");
        changeImage = luaInit.luaEnv.Global.Get<LuaDelegateConfig.Action_Image_string>("change_image");
        updateTimer = luaInit.luaEnv.Global.Get<LuaDelegateConfig.Action_TMP_Text_int>("update_timer");
        
        //1.改UI文本
        setText?.Invoke(helloText, "UI Text Changed by Lua");
        
        //2.按钮点击事件
        clilckButton.onClick.AddListener(() => onButtonClick?.Invoke(helloText));
        
        //3.改变图片（SpriteA 必须放在 Resources 文件夹）
        changeImage?.Invoke(targetImage, "SpriteA");
        
        //4.定时器
        InvokeRepeating(nameof(Tick), 0, 1);
    }
    
    void Tick()
    {
        if (timeLeft >= 0)
        {
            updateTimer?.Invoke(timerText, timeLeft);
            timeLeft--;
        }
        else
        {
            CancelInvoke(nameof(Tick));
        }
    }
}
