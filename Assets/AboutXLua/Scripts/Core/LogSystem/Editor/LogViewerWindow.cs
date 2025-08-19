using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LogViewerWindow : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<LogEntry> _filteredLogs = new List<LogEntry>();
    private string _searchSource = "";
    
    // 筛选选项
    private bool[] _scriptTypeFilters = { true, true }; // C#, Lua
    private bool[] _logLevelFilters;
    private bool[] _logLayerFilters;
    
    private GUIStyle _errorStyle;
    private GUIStyle _warningStyle;
    private GUIStyle _infoStyle;

    [MenuItem("XLua/Log Viewer")]
    public static void ShowWindow()
    {
        GetWindow<LogViewerWindow>("Log Viewer");
    }

    private void OnEnable()
    {
        // 初始化筛选器
        var levelValues = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToList();
        _logLevelFilters = new bool[levelValues.Count];
        Array.Fill(_logLevelFilters, true);
        
        var layerValues = Enum.GetValues(typeof(LogLayer)).Cast<LogLayer>().ToList();
        _logLayerFilters = new bool[layerValues.Count];
        Array.Fill(_logLayerFilters, true);
        
        // 注册日志更新事件
        LogUtility.OnLogAdded += UpdateLogs;
        
        // 初始化样式
        _errorStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } };
        _warningStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1f, 0.6f, 0f) } }; // 黄色
        _infoStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.white } };
        
        UpdateLogs();
    }

    private void OnDisable()
    {
        LogUtility.OnLogAdded -= UpdateLogs;
    }

    private void UpdateLogs()
    {
        var allLogs = LogUtility.LogEntries;
        _filteredLogs = FilterLogs(allLogs);
        Repaint();
    }

    private List<LogEntry> FilterLogs(IEnumerable<LogEntry> logs)
    {
        return logs.Where(log => 
            // 脚本类型筛选
            (_scriptTypeFilters[0] && log.ScriptType == "CSharp") || 
            (_scriptTypeFilters[1] && log.ScriptType == "Lua") &&
            
            // 日志级别筛选
            _logLevelFilters[(int)log.Level] &&
            
            // 日志层筛选
            _logLayerFilters[(int)log.Layer] &&
            
            // 源名称筛选
            (string.IsNullOrEmpty(_searchSource) || 
             log.Source.IndexOf(_searchSource, StringComparison.OrdinalIgnoreCase) >= 0)
        ).OrderByDescending(log => log.Time).ToList();
    }

    private void OnGUI()
    {
        DrawToolbar();
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        foreach (var log in _filteredLogs)
        {
            GUIStyle style = log.Level switch
            {
                LogLevel.Error => _errorStyle,
                LogLevel.Warning => _warningStyle,
                _ => _infoStyle
            };
            
            EditorGUILayout.LabelField($"{log.Time:HH:mm:ss} {log.FormattedMessage}", style);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        // 脚本类型筛选
        EditorGUILayout.LabelField("脚本类型:", GUILayout.Width(70));
        _scriptTypeFilters[0] = GUILayout.Toggle(_scriptTypeFilters[0], "CSharp", EditorStyles.toolbarButton, GUILayout.Width(60));
        _scriptTypeFilters[1] = GUILayout.Toggle(_scriptTypeFilters[1], "Lua", EditorStyles.toolbarButton, GUILayout.Width(40));
        
        GUILayout.Space(10);
        
        // 日志级别筛选
        EditorGUILayout.LabelField("日志级别:", GUILayout.Width(70));
        var levelValues = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToList();
        for (int i = 0; i < levelValues.Count; i++)
        {
            _logLevelFilters[i] = GUILayout.Toggle(_logLevelFilters[i], levelValues[i].ToString(), EditorStyles.toolbarButton);
        }
        
        GUILayout.Space(10);
        
        // 日志层筛选
        EditorGUILayout.LabelField("日志层:", GUILayout.Width(60));
        var layerValues = Enum.GetValues(typeof(LogLayer)).Cast<LogLayer>().ToList();
        for (int i = 0; i < layerValues.Count; i++)
        {
            _logLayerFilters[i] = GUILayout.Toggle(_logLayerFilters[i], layerValues[i].ToString(), EditorStyles.toolbarButton);
        }
        
        GUILayout.Space(10);
        
        // 源名称搜索
        EditorGUILayout.LabelField("源名称:", GUILayout.Width(60));
        _searchSource = EditorGUILayout.TextField(_searchSource, EditorStyles.toolbarTextField, GUILayout.MinWidth(100));
        
        GUILayout.FlexibleSpace();
        
        // 清除日志按钮
        if (GUILayout.Button("清除日志", EditorStyles.toolbarButton))
        {
            LogUtility.ClearLogs();
            _filteredLogs.Clear();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // 如果筛选条件变化，更新日志列表
        if (GUI.changed)
        {
            UpdateLogs();
        }
    }
}