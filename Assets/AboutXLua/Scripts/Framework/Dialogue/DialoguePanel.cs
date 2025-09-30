using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialoguePanel : UIFormBase
{
    [Header("UI组件")]
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI characterNameText;
    public Dictionary<string,SpriteRenderer> characterImageDic;
    public Transform optionsParent;
    public GameObject optionPrefab;
    public List<PosCanChoose> characterPos;
    
    // 现有选项按钮列表
    private List<GameObject> currentOptions = new List<GameObject>();
    
    /// <summary>
    /// 创建并显示选项
    /// </summary>
    public void CreateOptions(List<string> optionTexts, System.Action<int> onOptionSelected)
    {
        ClearOptions();
        
        if (optionTexts == null || optionPrefab == null || optionsParent == null) 
            return;
            
        for (int i = 0; i < optionTexts.Count; i++)
        {
            var optionObj = Instantiate(optionPrefab, optionsParent);
            var button = optionObj.GetComponent<Button>();
            var text = optionObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (text != null)
                text.text = optionTexts[i];
                
            int index = i; // 闭包捕获
            button.onClick.AddListener(() => onOptionSelected?.Invoke(index));
            
            currentOptions.Add(optionObj);
        }
        
        optionsParent.gameObject.SetActive(true);
    }

    /// <summary>
    /// 清空并隐藏选项
    /// </summary>
    public void ClearOptions()
    {
        foreach (var option in currentOptions)
        {
            if (option != null)
                Destroy(option);
        }
        currentOptions.Clear();
        
        if (optionsParent != null)
            optionsParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 更新角色状态
    /// </summary>
    /// <param name="characterNames">需要调整的角色名称</param>
    /// <param name="posAndOps">位置或快捷操作</param>
    public void UpdateCharacter(List<string> characterNames, List<string> posAndOps)
    {
        if (characterNames == null || characterNames.Count == 0) return;
        
        // 根据传入的参数对角色做对应的调整
        for (int i = 0; i < characterNames.Count && i < characterImageDic.Count; i++)
        {
            if (i < posAndOps.Count)
            {
                var operation = posAndOps[i];
                switch (operation)
                {
                    case "left":
                        SetCharacterPosition(characterNames[i], "left");
                        ShowCharacter(characterNames[i]);
                        break;
                    case "right":
                        SetCharacterPosition(characterNames[i], "right");
                        ShowCharacter(characterNames[i]);
                        break;
                    case "center":
                        SetCharacterPosition(characterNames[i], "center");
                        ShowCharacter(characterNames[i]);
                        break;
                    case "hide":
                        HideCharacter(characterNames[i]);
                        break;
                    case "show":
                        ShowCharacter(characterNames[i]);
                        break;
                }
            }
        }
    }
    
    private void SetCharacterPosition(string characterName, string pos)
    {
        
    }

    private void ShowCharacter(string characterName)
    {
        
    }
    
    private void HideCharacter(string characterName)
    {
        
    }
    
    public class PosCanChoose
    {
        public string pos;
        public Transform transform;
    }
    // 普通语句点击面板跳转下一句IPointerClickHandler接口经LuaUIEventBridge转接后由lua端实现
}
