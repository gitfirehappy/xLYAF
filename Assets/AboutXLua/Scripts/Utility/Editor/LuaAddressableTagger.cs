using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AboutXLua.Utility
{
    public class LuaAddressableTagger : EditorWindow
    {
        private LuaDataBase _luaDatabase;
        private List<LuaScriptContainer> _additionalContainers = new List<LuaScriptContainer>();
        private Vector2 _scrollPos;
        private Dictionary<LuaScriptContainer, bool> _containerFoldouts = new Dictionary<LuaScriptContainer, bool>();
        private Dictionary<LuaScriptContainer, Vector2> _labelScrolls = new Dictionary<LuaScriptContainer, Vector2>();
        private string _newLabel = "";
        private LuaScriptContainer _newContainer;

        [MenuItem("XLua/Addressable标签管理器", false, 101)]
        public static void ShowWindow()
        {
            GetWindow<LuaAddressableTagger>("Lua标签管理器");
        }

        private void OnGUI()
        {
            GUILayout.Label("Lua Addressable标签批量管理器", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // 数据库选择
            EditorGUILayout.BeginHorizontal();
            _luaDatabase = (LuaDataBase)EditorGUILayout.ObjectField(
                "Lua数据库", 
                _luaDatabase, 
                typeof(LuaDataBase), 
                false
            );
            
            if (GUILayout.Button("创建新数据库", GUILayout.Width(120)))
            {
                CreateNewDatabase();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 批量操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("应用所有标签", GUILayout.Height(30)))
            {
                ApplyAllLabels();
            }
            if (GUILayout.Button("清除所有标签", GUILayout.Height(30)))
            {
                ClearAllLabels();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            // 添加额外容器
            GUILayout.Label("额外容器", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _newContainer = (LuaScriptContainer)EditorGUILayout.ObjectField(
                _newContainer, 
                typeof(LuaScriptContainer), 
                false
            );
            
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                if (_newContainer != null && !_additionalContainers.Contains(_newContainer))
                {
                    _additionalContainers.Add(_newContainer);
                    _newContainer = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // 获取所有容器
            List<LuaScriptContainer> allContainers = GetAllContainers();
            
            if (allContainers.Count == 0)
            {
                EditorGUILayout.HelpBox("没有找到任何Lua容器", MessageType.Info);
                return;
            }

            // 容器列表
            GUILayout.Label($"容器列表 ({allContainers.Count}个)", EditorStyles.boldLabel);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(500));
            
            foreach (var container in allContainers)
            {
                if (container == null) continue;

                EditorGUILayout.BeginVertical("box");
                
                // 容器头部
                EditorGUILayout.BeginHorizontal();
                
                // 初始化折叠状态
                if (!_containerFoldouts.ContainsKey(container))
                {
                    _containerFoldouts[container] = true;
                }
                
                // 折叠箭头和容器名称
                _containerFoldouts[container] = EditorGUILayout.Foldout(
                    _containerFoldouts[container], 
                    $"{container.groupName} ({container.luaAssets.Count}个脚本)", 
                    true
                );
                
                GUILayout.FlexibleSpace();
                
                // 容器操作按钮
                if (GUILayout.Button("应用标签", GUILayout.Width(80)))
                {
                    ApplyContainerLabels(container);
                }
                if (GUILayout.Button("移除", GUILayout.Width(60)) && _additionalContainers.Contains(container))
                {
                    _additionalContainers.Remove(container);
                    _containerFoldouts.Remove(container);
                    _labelScrolls.Remove(container);
                }
                EditorGUILayout.EndHorizontal();

                // 容器内容（折叠显示）
                if (_containerFoldouts[container])
                {
                    EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    
                    // 组名编辑
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("组名:", GUILayout.Width(40));
                    container.groupName = EditorGUILayout.TextField(container.groupName);
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(5);
                    
                    // 标签管理
                    GUILayout.Label("地址标签:", EditorStyles.miniBoldLabel);
                    
                    if (!_labelScrolls.ContainsKey(container))
                    {
                        _labelScrolls[container] = Vector2.zero;
                    }
                    
                    _labelScrolls[container] = EditorGUILayout.BeginScrollView(
                        _labelScrolls[container], 
                        GUILayout.Height(100)
                    );
                    
                    // 显示现有标签
                    for (int i = 0; i < container.addressableLabels.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        container.addressableLabels[i] = EditorGUILayout.TextField(container.addressableLabels[i]);
                        
                        if (GUILayout.Button("×", GUILayout.Width(25)))
                        {
                            container.addressableLabels.RemoveAt(i);
                            i--;
                            EditorUtility.SetDirty(container);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                    
                    // 添加新标签
                    EditorGUILayout.BeginHorizontal();
                    _newLabel = EditorGUILayout.TextField("新标签", _newLabel);
                    
                    if (GUILayout.Button("添加", GUILayout.Width(60)) && !string.IsNullOrEmpty(_newLabel))
                    {
                        if (!container.addressableLabels.Contains(_newLabel))
                        {
                            container.addressableLabels.Add(_newLabel);
                            _newLabel = "";
                            EditorUtility.SetDirty(container);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    GUILayout.Space(10);
                    
                    // 脚本列表
                    GUILayout.Label($"脚本列表 ({container.luaAssets.Count}个)", EditorStyles.miniBoldLabel);
                    
                    for (int i = 0; i < container.luaAssets.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        container.luaAssets[i] = (TextAsset)EditorGUILayout.ObjectField(
                            container.luaAssets[i], 
                            typeof(TextAsset), 
                            false
                        );
                        
                        if (GUILayout.Button("×", GUILayout.Width(25)))
                        {
                            container.luaAssets.RemoveAt(i);
                            i--;
                            EditorUtility.SetDirty(container);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    
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
            int totalScripts = allContainers.Sum(c => c.luaAssets.Count);
            int totalLabels = allContainers.Sum(c => c.addressableLabels.Count);
            
            EditorGUILayout.HelpBox(
                $"总计: {allContainers.Count}个容器, {totalScripts}个脚本, {totalLabels}个标签", 
                MessageType.Info
            );
        }

        private List<LuaScriptContainer> GetAllContainers()
        {
            List<LuaScriptContainer> allContainers = new List<LuaScriptContainer>();
            
            // 添加数据库中的容器
            if (_luaDatabase != null)
            {
                allContainers.AddRange(_luaDatabase.groups.Where(c => c != null));
            }
            
            // 添加额外容器
            allContainers.AddRange(_additionalContainers.Where(c => c != null));
            
            return allContainers.Distinct().ToList();
        }

        private void CreateNewDatabase()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "创建Lua数据库",
                "LuaDatabase",
                "asset",
                "选择保存Lua数据库的位置"
            );

            if (!string.IsNullOrEmpty(path))
            {
                LuaDataBase newDatabase = CreateInstance<LuaDataBase>();
                AssetDatabase.CreateAsset(newDatabase, path);
                AssetDatabase.SaveAssets();
                _luaDatabase = newDatabase;
                Selection.activeObject = newDatabase;
            }
        }

        private void AddScriptsToContainer(LuaScriptContainer container)
        {
            string selectedPath = EditorUtility.OpenFilePanel(
                "选择Lua脚本", 
                Application.dataPath, 
                "lua"
            );

            if (string.IsNullOrEmpty(selectedPath)) return;

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

        private AddressableAssetGroup GetOrCreateAddressableGroup(string groupName, AddressableAssetSettings settings)
        {
            if (string.IsNullOrEmpty(groupName))
                groupName = "LuaScripts";
            
            // 查找现有组
            AddressableAssetGroup group = settings.groups.Find(g => g.Name == groupName);
            
            if (group == null)
            {
                // 创建新组
                group = settings.CreateGroup(groupName, false, false, true, null);
                
                // 设置新组的模式为Packed（可以根据需要调整）
                var schema = group.AddSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
                if (schema != null)
                {
                    schema.BuildPath.SetVariableByName(settings, AddressableAssetSettings.kLocalBuildPath);
                    schema.LoadPath.SetVariableByName(settings, AddressableAssetSettings.kLocalLoadPath);
                }
            }
            
            return group;
        }

        private void ApplyContainerLabels(LuaScriptContainer container)
        {
            if (container.luaAssets.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", $"容器 '{container.groupName}' 中没有脚本", "确定");
                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到Addressable设置，请先初始化Addressables", "确定");
                return;
            }

            // 获取或创建对应的Addressable组
            AddressableAssetGroup targetGroup = GetOrCreateAddressableGroup(container.groupName, settings);
            if (targetGroup == null)
            {
                EditorUtility.DisplayDialog("错误", $"无法创建或找到组: {container.groupName}", "确定");
                return;
            }

            int successCount = 0;
            
            foreach (TextAsset luaAsset in container.luaAssets)
            {
                if (luaAsset == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(luaAsset);
                AddressableAssetEntry entry = settings.FindAssetEntry(assetPath);
                
                if (entry == null)
                {
                    // 创建新的Addressable条目到目标组
                    entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), targetGroup);
                }
                else
                {
                    // 移动到目标组
                    settings.MoveEntry(entry, targetGroup);
                }

                if (entry != null)
                {
                    // 清除现有标签，然后应用新标签（覆盖逻辑）
                    List<string> currentLabels = entry.labels.ToList();
                    foreach (string label in currentLabels)
                    {
                        entry.SetLabel(label, false);
                    }
                    
                    // 应用容器中定义的所有标签
                    foreach (string label in container.addressableLabels)
                    {
                        if (!string.IsNullOrEmpty(label))
                        {
                            // 确保标签存在
                            if (!settings.GetLabels().Contains(label))
                            {
                                settings.AddLabel(label);
                            }
                            
                            // 添加标签到资源
                            entry.SetLabel(label, true);
                            successCount++;
                        }
                    }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.LabelAdded, null, true);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", 
                $"成功为容器 '{container.groupName}' 中的 {container.luaAssets.Count} 个脚本应用了标签", "确定");
        }

        private void ApplyAllLabels()
        {
            List<LuaScriptContainer> allContainers = GetAllContainers();
            
            if (allContainers.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有找到任何容器", "确定");
                return;
            }

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                EditorUtility.DisplayDialog("错误", "未找到Addressable设置，请先初始化Addressables", "确定");
                return;
            }

            int totalScriptsProcessed = 0;
            int containersProcessed = 0;

            foreach (var container in allContainers)
            {
                if (container == null || container.luaAssets.Count == 0)
                    continue;

                // 获取或创建对应的Addressable组
                AddressableAssetGroup targetGroup = GetOrCreateAddressableGroup(container.groupName, settings);
                if (targetGroup == null) continue;

                foreach (TextAsset luaAsset in container.luaAssets)
                {
                    if (luaAsset == null) continue;

                    string assetPath = AssetDatabase.GetAssetPath(luaAsset);
                    AddressableAssetEntry entry = settings.FindAssetEntry(assetPath);
                    
                    if (entry == null)
                    {
                        entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), targetGroup);
                    }
                    else
                    {
                        settings.MoveEntry(entry, targetGroup);
                    }

                    if (entry != null)
                    {
                        // 清除现有标签，然后应用新标签（覆盖逻辑）
                        List<string> currentLabels = entry.labels.ToList();
                        foreach (string label in currentLabels)
                        {
                            entry.SetLabel(label, false);
                        }
                        
                        // 应用容器中定义的所有标签
                        foreach (string label in container.addressableLabels)
                        {
                            if (!string.IsNullOrEmpty(label))
                            {
                                if (!settings.GetLabels().Contains(label))
                                {
                                    settings.AddLabel(label);
                                }
                                
                                entry.SetLabel(label, true);
                            }
                        }
                        
                        totalScriptsProcessed++;
                    }
                }

                containersProcessed++;
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.LabelAdded, null, true);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", 
                $"成功为 {containersProcessed} 个容器中的 {totalScriptsProcessed} 个脚本应用了标签", "确定");
        }

        private void ClearAllLabels()
        {
            if (!EditorUtility.DisplayDialog("确认", 
                "确定要清除所有Lua脚本的Addressable标签吗？此操作不可撤销。", "确定", "取消"))
            {
                return;
            }

            List<LuaScriptContainer> allContainers = GetAllContainers();
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            
            if (settings == null) return;

            int totalClearedCount = 0;

            foreach (var container in allContainers)
            {
                if (container == null || container.luaAssets.Count == 0) continue;

                foreach (TextAsset luaAsset in container.luaAssets)
                {
                    if (luaAsset == null) continue;

                    string assetPath = AssetDatabase.GetAssetPath(luaAsset);
                    AddressableAssetEntry entry = settings.FindAssetEntry(assetPath);
                    
                    if (entry != null)
                    {
                        // 获取当前所有标签的副本，然后清除
                        List<string> labelsToRemove = entry.labels.ToList();
                        foreach (string label in labelsToRemove)
                        {
                            entry.SetLabel(label, false);
                            totalClearedCount++;
                        }
                    }
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.LabelRemoved, null, true);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", 
                $"成功清除了 {totalClearedCount} 个标签", "确定");
        }
    }
}