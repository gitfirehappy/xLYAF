using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialoguePanel : UIFormBase,IPointerClickHandler
{
    [Header("UI组件")]
    public TextMeshProUGUI contentText;
    public TextMeshProUGUI characterNameText;
    public List<Image> characterImage;
    public Transform optionsParent;
    public GameObject optionPrefab;
    
    
    /// <summary>
    /// 创建选项
    /// </summary>
    public void CreateOptions()
    {
        
    }

    /// <summary>
    /// 清空选项
    /// </summary>
    public void ClearOptions()
    {
        
    }

    /// <summary>
    /// 更新角色状态
    /// </summary>
    /// <param name="characterName">需要调整的角色名称</param>
    /// <param name="posAndOp">位置或快捷操作</param>
    public void UpdateCharacter(string characterName, string posAndOp)
    {
        
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 普通对话类型点击面板继续
    }
}
