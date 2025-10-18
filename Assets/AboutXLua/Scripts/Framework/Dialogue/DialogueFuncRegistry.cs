using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XLua;

public static class DialogueFuncRegistry
{
    private static Dictionary<string, MethodInfo> _funcMap = new Dictionary<string, MethodInfo>();

    /// <summary>
    /// 只扫描实现了IDialogueFuncProvider接口的类
    /// </summary>
    public static void ScanAndRegister()
    {
        _funcMap.Clear();
        var assembly = Assembly.GetExecutingAssembly();
        
        // 只获取实现了IDialogueFuncProvider接口的类型
        var targetTypes = assembly.GetTypes()
            .Where(t => typeof(IDialogueFuncProvider).IsAssignableFrom(t) && !t.IsInterface);

        foreach (var type in targetTypes)
        {
            // 只处理静态方法
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<DialogueFuncAttribute>();
                if (attr != null)
                {
                    string funcName = !string.IsNullOrEmpty(attr.DisplayName) ? 
                        attr.DisplayName : method.Name;
                    if (string.IsNullOrEmpty(funcName))
                    {
                        Debug.LogWarning($"对话函数名不能为空：{type.Name}.{method.Name}");
                        continue;
                    }

                    if (_funcMap.ContainsKey(funcName))
                    {
                        Debug.LogWarning($"对话函数名重复：{funcName}（{type.Name}.{method.Name}）");
                        continue;
                    }

                    _funcMap.Add(funcName, method);
                    Debug.Log($"注册对话函数：{funcName} -> {type.Name}.{method.Name}");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取对话函数
    /// </summary>
    public static MethodInfo GetFunction(string funcName)
    {
        _funcMap.TryGetValue(funcName, out var method);
        return method;
    }

    [LuaCallCSharp]
    public static object InvokeFunction(string funcName, params object[] parameters)
    {
        
        var method = GetFunction(funcName);
        if (method == null)
        {
            Debug.LogError($"未找到对话函数：{funcName}");
            return null;
        }

        try
        {
            // TODO:处理XLua参数转换
            return method.Invoke(null, parameters);
        }
        catch (Exception e)
        {
            Debug.LogError($"执行对话函数出错 {funcName}：{e.Message}");
            return null;
        }
    }
}
