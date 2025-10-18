using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AboutXLua.Utility
{
    public class LuaBatchConverterWindow : EditorWindow
    {
        private List<LuaScriptContainer> _containers = new List<LuaScriptContainer>();
        private Vector2 _scrollPos;
        private Dictionary<LuaScriptContainer, bool> _containerFoldouts = new Dictionary<LuaScriptContainer, bool>();
        private Dictionary<LuaScriptContainer, Vector2> _containerScrolls = new Dictionary<LuaScriptContainer, Vector2>();
        private LuaScriptContainer _newContainer; // 用于添加新容器的临时字段

        [MenuItem("XLua/Batch Convert By Container", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<LuaBatchConverterWindow>(".lua <-> .lua.txt批量转换器");
        }

        private void OnGUI()
        {
            GUILayout.Label(".lua <-> .lua.txt批量转换器", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 批量转换按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("批量转换为.lua", GUILayout.Height(30)))
            {
                BatchConvertAll(".lua.txt", ".lua");
            }
            if (GUILayout.Button("批量转换为.lua.txt", GUILayout.Height(30)))
            {
                BatchConvertAll(".lua", ".lua.txt");
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 添加容器的UI
            GUILayout.Label("添加容器", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            // 使用ObjectField选择容器
            _newContainer = (LuaScriptContainer)EditorGUILayout.ObjectField(
                _newContainer, 
                typeof(LuaScriptContainer), 
                false
            );
            
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                if (_newContainer != null && !_containers.Contains(_newContainer))
                {
                    _containers.Add(_newContainer);
                    _newContainer = null; // 清空选择
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 容器列表操作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("清空容器列表"))
            {
                _containers.Clear();
                _containerFoldouts.Clear();
                _containerScrolls.Clear();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 容器列表
            GUILayout.Label($"容器列表 ({_containers.Count}个)", EditorStyles.boldLabel);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(400));
            
            for (int i = 0; i < _containers.Count; i++)
            {
                var container = _containers[i];
                if (container == null)
                {
                    _containers.RemoveAt(i);
                    i--;
                    continue;
                }

                EditorGUILayout.BeginVertical("box");
                
                // 容器头部
                EditorGUILayout.BeginHorizontal();
                
                // 初始化折叠状态
                if (!_containerFoldouts.ContainsKey(container))
                {
                    _containerFoldouts[container] = false;
                }
                
                // 折叠箭头和容器名称
                _containerFoldouts[container] = EditorGUILayout.Foldout(
                    _containerFoldouts[container], 
                    $"{container.name} ({container.luaAssets.Count}个脚本)", 
                    true
                );
                
                GUILayout.FlexibleSpace();
                
                // 容器操作按钮
                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    _containers.RemoveAt(i);
                    _containerFoldouts.Remove(container);
                    _containerScrolls.Remove(container);
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                
                EditorGUILayout.EndHorizontal();

                // 容器内容（折叠显示）
                if (_containerFoldouts[container])
                {
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    
                    // 脚本列表
                    if (!_containerScrolls.ContainsKey(container))
                    {
                        _containerScrolls[container] = Vector2.zero;
                    }
                    
                    _containerScrolls[container] = EditorGUILayout.BeginScrollView(
                        _containerScrolls[container], 
                        GUILayout.Height(200)
                    );
                    
                    for (int j = 0; j < container.luaAssets.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        container.luaAssets[j] = (TextAsset)EditorGUILayout.ObjectField(
                            container.luaAssets[j], 
                            typeof(TextAsset), 
                            false
                        );
                        
                        if (GUILayout.Button("×", GUILayout.Width(25)))
                        {
                            container.luaAssets.RemoveAt(j);
                            j--;
                            EditorUtility.SetDirty(container);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();

                    // 脚本操作按钮
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("添加脚本"))
                    {
                        AddScriptsToContainer(container);
                    }
                    if (GUILayout.Button("清空脚本"))
                    {
                        container.luaAssets.Clear();
                        EditorUtility.SetDirty(container);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.EndVertical();
                GUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();

            // 统计信息
            if (_containers.Count > 0)
            {
                int totalScripts = 0;
                foreach (var container in _containers)
                {
                    if (container != null)
                        totalScripts += container.luaAssets.Count;
                }
                
                EditorGUILayout.HelpBox(
                    $"总计: {_containers.Count}个容器, {totalScripts}个脚本", 
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox("请添加至少一个Lua脚本容器", MessageType.Info);
            }
        }

        private void AddScriptsToContainer(LuaScriptContainer container)
        {
            // 使用你的原始方法，选择单个文件
            string selectedPath = EditorUtility.OpenFilePanel(
                "选择Lua脚本", 
                Application.dataPath, 
                "lua"
            );

            if (string.IsNullOrEmpty(selectedPath)) return;

            // 将单个路径转换为数组，以便统一处理
            string[] paths = new string[] { selectedPath };

            foreach (string path in paths)
            {
                if (path.StartsWith(Application.dataPath))
                {
                    string assetPath = "Assets" + path.Substring(Application.dataPath.Length);
                    TextAsset luaAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    
                    if (luaAsset != null && !container.luaAssets.Contains(luaAsset))
                    {
                        container.luaAssets.Add(luaAsset);
                    }
                }
            }
            EditorUtility.SetDirty(container);
        }

        private void BatchConvertAll(string oldExt, string newExt)
        {
            if (_containers.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有选择任何容器", "确定");
                return;
            }

            int totalSuccessCount = 0;
            int totalContainersProcessed = 0;

            AssetDatabase.StartAssetEditing();
            
            foreach (var container in _containers)
            {
                if (container == null || container.luaAssets.Count == 0) continue;

                int containerSuccessCount = 0;
                List<TextAsset> newAssets = new List<TextAsset>();
                List<string> successNewPaths = new List<string>();

                foreach (TextAsset asset in container.luaAssets)
                {
                    if (asset == null) continue;

                    string path = AssetDatabase.GetAssetPath(asset);
                    // 处理不需要转换的文件（直接保留原引用）
                    if (!path.EndsWith(oldExt, System.StringComparison.OrdinalIgnoreCase))
                    {
                        newAssets.Add(asset);
                        continue;
                    }

                    // 计算新路径并移动文件
                    string newPath = path.Substring(0, path.Length - oldExt.Length) + newExt;
                    string moveResult = AssetDatabase.MoveAsset(path, newPath);
            
                    if (string.IsNullOrEmpty(moveResult)) // 移动成功
                    {
                        successNewPaths.Add(newPath);
                        containerSuccessCount++;
                    }
                    else // 移动失败
                    {
                        newAssets.Add(asset); // 保留原引用
                        Debug.LogError($"转换失败 {asset.name}: {moveResult}");
                    }
                }

                // 刷新后加载成功转换的文件
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.StartAssetEditing();

                foreach (string newPath in successNewPaths)
                {
                    TextAsset newAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(newPath);
                    if (newAsset != null)
                    {
                        newAssets.Add(newAsset);
                    }
                }

                // 更新容器列表
                container.luaAssets.Clear();
                container.luaAssets.AddRange(newAssets);
                EditorUtility.SetDirty(container);

                totalSuccessCount += containerSuccessCount;
                totalContainersProcessed++;
            }
            
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "完成", 
                $"成功转换 {totalSuccessCount} 个文件，处理了 {totalContainersProcessed} 个容器", 
                "确定"
            );
        }
    }
}