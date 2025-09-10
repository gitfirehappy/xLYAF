using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JsonReader : IConfigReader
{
    public ConfigFormat SupportedFormat => ConfigFormat.Json;

    public ConfigData Read(string filePath)
    {
        var configData = new ConfigData();
        configData.PrimitiveFormat = ConfigFormat.Json;

        // 读取JSON文件内容
        string jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

        try
        {
            // 尝试解析为数组格式
            if (jsonContent.Trim().StartsWith("["))
            {
                return ParseArrayFormat(jsonContent, configData);
            }
            // 尝试解析为对象格式（键值对）
            else if (jsonContent.Trim().StartsWith("{"))
            {
                return ParseObjectFormat(jsonContent, configData);
            }
            else
            {
                LogUtility.Error(LogLayer.Framework, "JsonReader", $"不支持的JSON格式: {filePath}");
                return configData;
            }
        }
        catch (System.Exception ex)
        {
            LogUtility.Error(LogLayer.Framework, "JsonReader", $"解析JSON文件时出错: {filePath}\n{ex.Message}");
            return configData;
        }
    }

    #region 解析JSON

    /// <summary>
    /// 解析数组格式的JSON
    /// </summary>
    private ConfigData ParseArrayFormat(string jsonContent, ConfigData configData)
    {
        var rows = new List<object[]>();

        // 使用SimpleJSON解析JSON数组
        var jsonArray = SimpleJSON.JSON.Parse(jsonContent).AsArray;

        if (jsonArray == null || jsonArray.Count == 0)
        {
            LogUtility.Warning(LogLayer.Framework, "JsonReader", "JSON数组为空或格式不正确");
            return configData;
        }

        // 从第一个对象获取所有字段名
        var firstItem = jsonArray[0].AsObject;
        List<string> columns = new List<string>();

        // 获取所有键
        foreach (var key in firstItem.Keys)
        {
            columns.Add(key);
        }

        configData.Columns = columns.ToArray();

        // 处理每一行数据
        for (int i = 0; i < jsonArray.Count; i++)
        {
            var item = jsonArray[i].AsObject;
            object[] row = new object[columns.Count];

            for (int j = 0; j < columns.Count; j++)
            {
                string columnName = columns[j];
                if (item.HasKey(columnName))
                {
                    var value = item[columnName];
                    // 根据值的类型进行处理
                    if (value.IsString)
                        row[j] = value.Value;
                    else if (value.IsNumber)
                        row[j] = value.AsFloat;
                    else if (value.IsBoolean)
                        row[j] = value.AsBool;
                    else
                        row[j] = value.Value; // 默认转为字符串
                }
                else
                {
                    row[j] = null;
                    LogUtility.Warning(LogLayer.Framework, "JsonReader", $"字段 '{columnName}' 在第 {i + 1} 个对象中不存在");
                }
            }

            rows.Add(row);
        }

        configData.Rows = rows;
        return configData;
    }

    /// <summary>
    /// 解析对象格式的JSON（键值对）
    /// </summary>
    private ConfigData ParseObjectFormat(string jsonContent, ConfigData configData)
    {
        var rows = new List<object[]>();

        // 使用SimpleJSON解析JSON对象
        var jsonObject = SimpleJSON.JSON.Parse(jsonContent).AsObject;

        if (jsonObject == null || jsonObject.Count == 0)
        {
            LogUtility.Warning(LogLayer.Framework, "JsonReader", "JSON对象为空或格式不正确");
            return configData;
        }

        // 获取所有字段名（从第一个值中获取）
        List<string> keys = new List<string>();
        foreach (var key in jsonObject.Keys)
        {
            keys.Add(key);
        }

        string firstKey = keys[0];
        var firstValue = jsonObject[firstKey].AsObject;

        List<string> columns = new List<string> { "id" }; // ID作为第一列

        List<string> fieldKeys = new List<string>();
        foreach (var key in firstValue.Keys)
        {
            fieldKeys.Add(key);
        }

        foreach (var key in fieldKeys)
        {
            columns.Add(key);
        }

        configData.Columns = columns.ToArray();

        // 处理每一行数据
        foreach (var key in keys)
        {
            var value = jsonObject[key].AsObject;
            object[] row = new object[columns.Count];
            row[0] = key; // ID

            for (int i = 1; i < columns.Count; i++)
            {
                string columnName = columns[i];
                if (value.HasKey(columnName))
                {
                    // 根据值的类型进行处理
                    var fieldValue = value[columnName];
                    if (fieldValue.IsString)
                        row[i] = fieldValue.Value;
                    else if (fieldValue.IsNumber)
                        row[i] = fieldValue.AsFloat;
                    else if (fieldValue.IsBoolean)
                        row[i] = fieldValue.AsBool;
                    else
                        row[i] = fieldValue.Value;
                }
                else
                {
                    row[i] = null;
                    LogUtility.Warning(LogLayer.Framework, "JsonReader", $"字段 '{columnName}' 在对象 '{key}' 中不存在");
                }
            }

            rows.Add(row);
        }

        configData.Rows = rows;
        return configData;
    }

    #endregion
}