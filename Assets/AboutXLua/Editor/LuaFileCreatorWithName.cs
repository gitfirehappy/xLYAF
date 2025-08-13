using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AboutXLua.Editor
{
    public class LuaFileCreatorWithName
    {
        private const string LuaTemplate = "-- Lua script\n\nfunction Start()\n    print(\"Hello Lua\")\nend";
    
        // 合并创建菜单到多级菜单
        [MenuItem("Assets/Create/Lua Script/Standard (.lua)", false, 80)]
        private static void CreateLuaFile() => CreateLuaFileWithExtension(".lua");

        [MenuItem("Assets/Create/Lua Script/Text Format (.lua.txt)", false, 81)]
        private static void CreateLuaTxtFile() => CreateLuaFileWithExtension(".lua.txt");

        // 使用更具识别度的菜单名称
        [MenuItem("Assets/Lua Tools/Convert Format/Lua → Lua.txt", true, 30)]
        [MenuItem("Assets/Lua Tools/Convert Format/Lua.txt → Lua", true, 31)]
        private static bool ValidateConvertMenu()
        {
            return Selection.activeObject != null && 
                   !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        [MenuItem("Assets/Lua Tools/Convert Format/Lua → Lua.txt", false, 30)]
        private static void ConvertLuaToTxt()
        {
            ConvertSelectedFiles(extension: ".lua", newExtension: ".lua.txt");
        }

        [MenuItem("Assets/Lua Tools/Convert Format/Lua.txt → Lua", false, 31)]
        private static void ConvertTxtToLua()
        {
            ConvertSelectedFiles(extension: ".lua.txt", newExtension: ".lua");
        }

        /// <summary>
        /// 创建Lua脚本
        /// </summary>
        private static void CreateLuaFileWithExtension(string extension)
        {
            string path = GetSelectedPathOrFallback();
            string defaultName = "NewLuaScript";
            string fileName = EditorUtility.SaveFilePanel(
                $"Create {extension} Script",
                path,
                defaultName + extension,
                extension.Replace(".", "")
            );

            if (string.IsNullOrEmpty(fileName)) 
                return;

            File.WriteAllText(fileName, LuaTemplate);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(fileName.Replace(Application.dataPath, "Assets"));
        }

        /// <summary>
        /// 文件类型转换
        /// </summary>
        private static void ConvertSelectedFiles(string extension, string newExtension)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
        
                foreach (var obj in Selection.objects)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (string.IsNullOrEmpty(path) || Directory.Exists(path)) 
                        continue;

                    // 检查扩展名（不区分大小写）
                    if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // 智能生成新路径：移除整个旧扩展名，然后添加新扩展名
                    string basePath = path.Substring(0, path.Length - extension.Length);
                    string newPath = basePath + newExtension;
            
                    string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), path);
                    string newFullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), newPath);

                    if (File.Exists(newFullPath))
                    {
                        Debug.LogWarning($"Skipped {path}: Target file already exists");
                        continue;
                    }

                    // 执行文件重命名
                    AssetDatabase.MoveAsset(path, newPath);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 获取选择路径
        /// </summary>
        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    break;
                }
            }
            return path;
        }
    }
}