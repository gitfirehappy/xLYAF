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
    public GameObject optionsParent;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
