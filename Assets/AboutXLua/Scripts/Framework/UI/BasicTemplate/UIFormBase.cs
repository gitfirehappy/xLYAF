using UnityEngine;
using System;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 所有 UI 面板应继承此类，内置动画、生命周期控制
/// </summary>
public class UIFormBase : MonoBehaviour, IUIForm
{
    protected UIManager uIManager;

    public bool IsOpen = false;
    public bool IsInited = false;

    [SerializeField]
    private UIFormConfigSO _config;
    public int MajorLayerOrder => _config != null ? _config.majorOrder : 0;
    public int MinorLayerOrder => _config != null ? _config.minorOrder : 0;
    public bool Cached => _config != null ? _config.cached : false;
    public FormAnimType AnimType => _config != null ? _config.animType : FormAnimType.None;

    public Vector3 originalLocalPos;

    [Header("动态面板配置")]
    [SerializeField] private bool isDynamicForm = false;
    [SerializeField] private string dynamicGroupID = "";

    public bool IsDynamicForm => isDynamicForm;
    public string DynamicGroupID => dynamicGroupID;


    private void Awake()
    {
        if (!gameObject.scene.IsValid()) return;

        IUIForm ui = this;
        ui.RegisterForm();

        originalLocalPos = transform.localPosition; // 缓存初始位置

        // 场景面板默认关闭，不闪烁
        if (gameObject.activeSelf)
        {
            CloseImmediate();
        }

        // Fade面板初始化CanvasGroup alpha为0，保证第一次打开时能淡入
        if (AnimType == FormAnimType.Fade)
        {
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = gameObject.AddComponent<CanvasGroup>();
            }
            cg.alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        IUIForm ui = this;
        ui.UnRegisterForm();
    }

    public void Open(UIManager uIManager)
    {
        this.uIManager = uIManager;
        if (!IsInited)
        {
            IsInited = true;
            Init(); // 首次打开时初始化
        }

        OpenAnim();
    }

    public void Close()
    {
        if (!IsOpen) return;

        CloseAnim();
    }

    /// <summary>
    /// 直接关闭且不播放动画，内部调用（场景面板默认关闭用）
    /// </summary>
    private void CloseImmediate()
    {
        // 若是Fade动画，关闭时alpha也要重置为0，确保下次打开能淡入
        if (AnimType == FormAnimType.Fade)
        {
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        gameObject.SetActive(false);
        IsOpen = false;
    }

    /// <summary>
    /// 初始化方法，面板逻辑初始化应重写此方法
    /// </summary>
    protected virtual void Init() { }

    private void OpenAnim()
    {
        switch (AnimType)
        {
            case FormAnimType.None:
                gameObject.SetActive(true);
                IsOpen = true;
                break;
            case FormAnimType.Fade:
                UIAnimation.FadeIn(this, () => IsOpen = true);
                break;
            case FormAnimType.Zoom:
                UIAnimation.ZoomIn(this, () => IsOpen = true);
                break;
            case FormAnimType.Pop:
                UIAnimation.PopIn(this, () => IsOpen = true);
                break;
            case FormAnimType.SlideLeft:
                UIAnimation.SlideIn(this, new Vector3(-Screen.width, 0, 0), () => IsOpen = true);
                break;
            case FormAnimType.SlideRight:
                UIAnimation.SlideIn(this, new Vector3(Screen.width, 0, 0), () => IsOpen = true);
                break;
            case FormAnimType.SlideUp:
                UIAnimation.SlideIn(this, new Vector3(0, Screen.height, 0), () => IsOpen = true);
                break;
            case FormAnimType.SlideDown:
                UIAnimation.SlideIn(this, new Vector3(0, -Screen.height, 0), () => IsOpen = true);
                break;
            case FormAnimType.FadeSlide:
                UIAnimation.FadeSlideIn(this, new Vector3(0, -100, 0), () => IsOpen = true);
                break;
        }
    }

    private void CloseAnim()
    {
        Action onCloseComplete = () =>
        {
            IsOpen = false;

            if (!Cached)
            {
                UIManager.Instance.UnRegisterForm(this);
                Destroy(gameObject);
            }
        };

        switch (AnimType)
        {
            case FormAnimType.None:
                gameObject.SetActive(false);
                onCloseComplete();
                break;
            case FormAnimType.Fade:
                UIAnimation.FadeOut(this, onCloseComplete);
                break;
            case FormAnimType.Zoom:
                UIAnimation.ZoomOut(this, onCloseComplete);
                break;
            case FormAnimType.Pop:
                UIAnimation.PopOut(this, onCloseComplete);
                break;
            case FormAnimType.SlideLeft:
                UIAnimation.SlideOut(this, new Vector3(-Screen.width, 0, 0), onCloseComplete);
                break;
            case FormAnimType.SlideRight:
                UIAnimation.SlideOut(this, new Vector3(Screen.width, 0, 0), onCloseComplete);
                break;
            case FormAnimType.SlideUp:
                UIAnimation.SlideOut(this, new Vector3(0, Screen.height, 0), onCloseComplete);
                break;
            case FormAnimType.SlideDown:
                UIAnimation.SlideOut(this, new Vector3(0, -Screen.height, 0), onCloseComplete);
                break;
            case FormAnimType.FadeSlide:
                UIAnimation.FadeOut(this, onCloseComplete); // 可以扩展加 SlideOut 效果
                break;
        }
    }

    public UIFormBase GetUIFormBase() => this;

    #region 动态生成面板扩展

    public void InitializeAsDynamic(string groupID, UIFormConfigSO config = null)
    {
        isDynamicForm = true;
        dynamicGroupID = groupID;
        if (config != null) _config = config;

        // 如果是动态面板，确保注册到UIManager
        if (!IsInited)
        {
            IsInited = true;
            Init();
        }
    }

    protected Canvas FindCanvasForGroup(string groupID)
    {
        Canvas canvas = UIManager.Instance?.GetCanvasGroup(groupID);
        if (canvas == null)
        {
            Debug.LogError($"Canvas for group '{groupID}' not found!");
        }
        return canvas;
    }

    #endregion

}
