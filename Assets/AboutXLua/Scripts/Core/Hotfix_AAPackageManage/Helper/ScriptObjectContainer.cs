using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOContainer", menuName = "Addressables/MyWork/SO Container")]
public class ScriptObjectContainer : ScriptableObject
{
    [Header("组设置")]
    [Tooltip("容器组名，用于Addressable分组")]
    public string groupName = "NewSOGroup";

    [Header("标签管理")]
    [Tooltip("要应用到SO资源的Addressable标签")]
    public List<string> addressableLabels = new();

    [Header("SO资源")]
    [Tooltip("要管理的ScriptableObject资源列表")]
    public List<ScriptableObject> soAssets = new();
}
