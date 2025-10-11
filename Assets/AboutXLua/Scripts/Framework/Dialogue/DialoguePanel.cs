using System;
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
    public Transform optionsParent;
    public GameObject optionPrefab;
    public int characterImageSortingLayer;
    
    [Header("配置")]
    [SerializeField] private List<CharacterImageSources> characterImages;
    [SerializeField] private List<PosCanChoose> characterPos;
    
    private List<GameObject> currentOptions = new List<GameObject>();
    
    ///<summary>名字 -> 当前状态 </summary>
    private Dictionary<string, CharacterRuntimeState> activeCharacters = new();
    
    ///<summary>位置 -> 角色名 </summary>>
    private Dictionary<string, string> positionOccupancy = new();
    
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
        for (int i = 0; i < characterNames.Count && i < characterImages.Count; i++)
        {
            if (i < posAndOps.Count)
            {
                var operations = ParseOperations(posAndOps[i]);
                foreach (var operation in operations)
                {
                    ExecuteOperation(characterNames[i], operation);
                }
            }
        }
    }
    
    /// <summary>
    /// 解析操作字符串，支持'&'分隔的多个操作
    /// </summary>
    private List<string> ParseOperations(string operationString)
    {
        var operations = new List<string>();
        if (string.IsNullOrEmpty(operationString)) return operations;
        
        var parts = operationString.Split('&');
        foreach (var part in parts)
        {
            operations.Add(part.Trim());
        }
        return operations;
    }
    
    /// <summary>
    /// 执行单个操作
    /// </summary>
    private void ExecuteOperation(string characterName, string operation)
    {
        switch (operation.ToLower())
        {
            case "left":
                SetCharacterPosition(characterName, "left");
                ShowCharacter(characterName);
                break;
            case "right":
                SetCharacterPosition(characterName, "right");
                ShowCharacter(characterName);
                break;
            case "center":
                SetCharacterPosition(characterName, "center");
                ShowCharacter(characterName);
                break;
            case "hide":
                HideCharacter(characterName);
                break;
            case "show":
                ShowCharacter(characterName);
                break;
            default:
                // 检查是否为图片差分操作（diff+数字）
                if (operation.StartsWith("diff"))
                {
                    SetCharacterExpression(characterName, operation);
                }
                break;
        }
    }

    #region 具体快捷操作
    
    private void SetCharacterPosition(string characterName, string pos)
    {
        // 查找角色配置
        var characterConfig = characterImages.Find(c => c.Name == characterName);
        if (characterConfig == null)
        {
            Debug.LogWarning($"未找到角色配置: {characterName}");
            return;
        }
        
        // 查找位置配置
        var posConfig = characterPos.Find(p => p.pos.ToLower() == pos.ToLower());
        if (posConfig == null || posConfig.transform == null)
        {
            Debug.LogWarning($"未找到位置配置: {pos}");
            return;
        }
        
        // 获取或创建运行时状态
        if (!activeCharacters.TryGetValue(characterName, out var runtimeState))
        {
            runtimeState = new CharacterRuntimeState
            {
                CharacterName = characterName,
                CurrentRenderer = CreateCharacterRenderer(characterName, characterConfig.Images[0])
            };
            activeCharacters[characterName] = runtimeState;
        }
        
        // 处理位置占用
        if (!string.IsNullOrEmpty(runtimeState.CurrentPos))
        {
            positionOccupancy.Remove(runtimeState.CurrentPos);
        }
        
        // 设置新位置
        runtimeState.CurrentRenderer.transform.position = posConfig.transform.position;
        runtimeState.CurrentPos = pos;
        positionOccupancy[pos] = characterName;
    }

    private void ShowCharacter(string characterName)
    {
        // 查找角色配置
        var characterConfig = characterImages.Find(c => c.Name == characterName);
        if (characterConfig == null)
        {
            Debug.LogWarning($"未找到角色配置: {characterName}");
            return;
        }
        
        // 获取或创建运行时状态
        if (!activeCharacters.TryGetValue(characterName, out var runtimeState))
        {
            runtimeState = new CharacterRuntimeState
            {
                CharacterName = characterName,
                CurrentRenderer = CreateCharacterRenderer(characterName, characterConfig.Images[0])
            };
            activeCharacters[characterName] = runtimeState;
        }
        
        // 显示角色
        runtimeState.CurrentRenderer.gameObject.SetActive(true);
    }
    
    private void HideCharacter(string characterName)
    {
        if (activeCharacters.TryGetValue(characterName, out var runtimeState))
        {
            // 隐藏角色
            runtimeState.CurrentRenderer.gameObject.SetActive(false);
            
            // 释放位置占用
            if (!string.IsNullOrEmpty(runtimeState.CurrentPos))
            {
                positionOccupancy.Remove(runtimeState.CurrentPos);
                runtimeState.CurrentPos = null;
            }
            
            // 可选：完全移除运行时状态
            // activeCharacters.Remove(characterName);
        }
    }
    
    /// <summary>
    /// 设置角色图片差分
    /// </summary>
    /// <param name="characterName"></param>
    /// <param name="operation"></param>
    private void SetCharacterExpression(string characterName, string operation)
    {
        
    }
    
    /// <summary>
    /// 创建角色渲染器
    /// </summary>
    private SpriteRenderer CreateCharacterRenderer(string characterName, Sprite defaultSprite)
    {
        var go = new GameObject($"Character_{characterName}");
        go.transform.SetParent(transform); // 作为面板子对象
        
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = defaultSprite;
        renderer.sortingOrder = characterImageSortingLayer; // 设置合适的渲染层级
        
        return renderer;
    }
    
    #endregion

    #region 复合结构
    
    [Serializable]
    public class PosCanChoose
    {
        public string pos;
        public Transform transform;
    }

    [Serializable]
    public class CharacterImageSources
    {
        public string Name;
        public List<Sprite> Images;
    }
    
    /// <summary>
    /// 当前活跃的角色状态
    /// </summary> 
    private class CharacterRuntimeState
    {
        public string CharacterName;
        public SpriteRenderer CurrentRenderer;
        public string CurrentPos = "center";
        public int CurrentDiffIndex = 0;
    }
    
    #endregion
    
    /// <summary>
    /// 清理所有角色状态
    /// </summary>
    public void ClearAllCharacters()
    {
        foreach (var runtimeState in activeCharacters.Values)
        {
            if (runtimeState.CurrentRenderer != null)
            {
                Destroy(runtimeState.CurrentRenderer.gameObject);
            }
        }
        
        activeCharacters.Clear();
        positionOccupancy.Clear();
    }
    
    // 普通语句点击面板跳转下一句IPointerClickHandler接口经LuaUIEventBridge转接后由lua端实现
}
