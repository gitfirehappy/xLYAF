using UnityEngine;

[CreateAssetMenu(fileName = "UIFormConfig", menuName = "UI/UI Form Config", order = 1)]
public class UIFormConfigSO : ScriptableObject
{
    [Header("显示名称")]
    public string displayName = "Default";

    [Header("层级配置")]
    public int majorOrder = 0;  // 大层级数字（主排序）
    public int minorOrder = 0;  // 小层级数字（次排序）

    [Header("行为配置")]
    public bool cached = false; // 是否缓存（不销毁）
    public FormAnimType animType = FormAnimType.None; // 动画类型
}