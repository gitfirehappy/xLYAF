using UnityEngine;
using UnityEditor;
using System.IO;

public class LuaFileCreatorWithName
{
    [MenuItem("Assets/Create/Lua Script", false, 80)]
    private static void CreateLuaFile()
    {
        // 获取选中路径
        string path = GetSelectedPathOrFallback();

        // 弹窗输入文件名（默认值 NewLuaScript）
        string fileName = EditorUtility.SaveFilePanelInProject(
            "Create Lua Script",
            "NewLuaScript",
            "lua",
            "请输入 Lua 文件名",
            path
        );

        // 如果用户取消，直接退出
        if (string.IsNullOrEmpty(fileName))
            return;

        // 写入默认模板
        File.WriteAllText(fileName, "-- Lua script\n\nfunction Start()\n    print(\"Hello Lua\")\nend");

        // 刷新资源
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取选中路径，如果没选中，就返回 "Assets"
    /// </summary>
    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}