using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[Serializable]
public class TypeReference
{
    public string assemblyName;
    public string typeName;
    public List<TypeReference> genericArguments = new List<TypeReference>();

    [NonSerialized] private Type _cachedType;
    [NonSerialized] private bool _isTypeCached;

    public Type GetTypeCache()
    {
        if (_isTypeCached) return _cachedType;
        
        try
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName))
                return CacheAndReturn(null);
            
            Assembly assembly = LoadAssembly(assemblyName);
            if (assembly == null)
                return CacheAndReturn(null);
            
            Type type = assembly.GetType(typeName);
            if (type == null)
            {
                Debug.LogWarning($"Type not found: {typeName} in {assemblyName}");
                return CacheAndReturn(null);
            }
            
            // 处理泛型类型
            if (genericArguments.Count > 0 && type.IsGenericTypeDefinition)
            {
                Type[] typeArgs = ResolveGenericArguments();
                if (typeArgs != null && typeArgs.Length == type.GetGenericArguments().Length)
                {
                    type = type.MakeGenericType(typeArgs);
                }
            }
            
            return CacheAndReturn(type);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading type {typeName}: {ex.Message}");
            return CacheAndReturn(null);
        }
    }

    private Type CacheAndReturn(Type type)
    {
        _cachedType = type;
        _isTypeCached = true;
        return type;
    }

    private Assembly LoadAssembly(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Assembly load failed: {assemblyName}. Error: {ex.Message}");
            return null;
        }
    }

    private Type[] ResolveGenericArguments()
    {
        var resolvedArgs = new List<Type>();
        foreach (var argRef in genericArguments)
        {
            Type argType = argRef.GetTypeCache();
            if (argType == null)
            {
                Debug.LogError($"Failed to resolve generic argument: {argRef}");
                return null;
            }
            resolvedArgs.Add(argType);
        }
        return resolvedArgs.ToArray();
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(typeName)) 
            return "None";
        
        string baseName = $"{typeName}, {assemblyName}";
        
        // 添加泛型参数信息
        if (genericArguments.Count > 0)
        {
            string args = string.Join(", ", genericArguments);
            return $"{baseName}<{args}>";
        }
        return baseName;
    }
}