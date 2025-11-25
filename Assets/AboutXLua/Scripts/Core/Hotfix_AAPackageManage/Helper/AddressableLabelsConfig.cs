using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 构建期导出的 AA 条目配置
/// </summary>
[CreateAssetMenu(fileName = "AddressableLabelsConfig", menuName = "Addressables/MyWork/扫描导出的PackageEntries")]
public class AddressableLabelsConfig : ScriptableObject
{
    public List<PackageEntry> allEntries = new();
}
