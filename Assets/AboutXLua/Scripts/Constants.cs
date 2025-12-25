using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有 string 常量
/// </summary>
public static class Constants
{
    /// <summary> 项目名 </summary>
    public const string PROJECTNAME = "ProjectName";
    
    /// <summary> 导出的AA配置条目的 Key </summary>
    public const string AA_LABELS_CONFIG = "AddressableLabelsConfig";
    
    /// <summary> 本地整包辅助构建数据组名 </summary>
    public const string LOCAL_STATUS_GROUP_NAME = "LocalStaticData";

    /// <summary> 本地整包构建索引文件路径 </summary>
    public const string BUILD_INDEX_ASSETPATH = "Assets/Build/LocalStaticData/BuildIndex.asset";

    /// <summary> 远端辅助构建数据组名 </summary>
    public const string HELPER_BUILD_DATA_GROUP_NAME = "HelperBuildData";

    /// <summary> AA条目配置文件路径 </summary>
    public const string AA_LABELS_CONFIG_ASSETPATH = "Assets/Build/HelperBuildData/AddressableLabelsConfig.asset";

    /// <summary> Lua 脚本索引的 Key </summary>
    public const string LUA_SCRIPTS_INDEX = "LuaScriptsIndex";
    
    /// <summary> Lua 脚本索引文件路径 </summary>
    public const string LUA_SCRIPTS_INDEX_ASSETPATH = "Assets/Build/HelperBuildData/LuaScriptsIndex.asset";
}
