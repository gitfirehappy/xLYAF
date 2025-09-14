using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleXML;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

public class XmlReader : IConfigReader
{
    public ConfigFormat SupportedFormat => ConfigFormat.Xml;

    public ConfigData Read(string filePath)
    {
        var configData = new ConfigData();
        configData.PrimitiveFormat = ConfigFormat.Xml;

        try
        {
            // 读取XML文件内容
            string xmlContent = File.ReadAllText(filePath, Encoding.UTF8);
            XMLNode rootNode = XMLNode.Parse(xmlContent);

            // 检查XML格式类型
            if (IsTableFormat(rootNode))
            {
                return ParseTableFormat(rootNode, configData);
            }
            else if (IsKeyValueFormat(rootNode))
            {
                return ParseKeyValueFormat(rootNode, configData);
            }
            else
            {
                LogUtility.Error(LogLayer.Framework, "XmlReader", $"不支持的XML格式: {filePath}");
                return configData;
            }
        }
        catch (System.Exception ex)
        {
            LogUtility.Error(LogLayer.Framework, "XmlReader", $"解析XML文件时出错: {filePath}\n{ex.Message}");
            return configData;
        }
    }

    #region 检查XML格式

    /// <summary>
    /// 检查XML格式是否为表格格式
    /// </summary>
    private bool IsTableFormat(XMLNode rootNode)
    {
        if (!rootNode.IsObject)
            return false;
        
        var obj = rootNode.AsObject;
        if (obj.Count == 0)
            return false;
        
        // 查找第一个数组类型的子节点
        foreach (var key in obj.Keys)
        {
            if (!key.StartsWith("@") && obj[key].IsArray)
            {
                return true;
            }
        }
    
        return false;
    }

    /// <summary>
    /// 检查XML格式是否为键值对格式
    /// </summary>
    private bool IsKeyValueFormat(XMLNode rootNode)
    {
        if (!rootNode.IsObject)
            return false;
        
        var obj = rootNode.AsObject;
        if (obj.Count == 0)
            return false;
        
        // 检查是否所有子节点都是对象且名称不同
        bool hasObjectChild = false;
        HashSet<string> childNames = new HashSet<string>();
    
        foreach (var key in obj.Keys)
        {
            if (!key.StartsWith("@"))
            {
                if (obj[key].IsObject)
                {
                    hasObjectChild = true;
                
                    // 检查名称是否唯一
                    if (childNames.Contains(key))
                        return false;
                    childNames.Add(key);
                }
            }
        }
    
        return hasObjectChild;
    }

    #endregion

    #region 解析XML

    /// <summary>
    /// 解析表格格式的XML文件
    /// 示例格式:
    /// <root>
    ///   <item><id>1</id><name>A</name></item>
    ///   <item><id>2</id><name>B</name></item>
    /// </root>
    /// </summary>
    private ConfigData ParseTableFormat(XMLNode rootNode, ConfigData configData)
    {
        var rows = new List<object[]>();
        var obj = rootNode.AsObject;
        
        // 查找数组类型的子节点
        string arrayNodeName = null;
        XMLArray arrayNode = null;
    
        foreach (var key in obj.Keys)
        {
            if (!key.StartsWith("@") && obj[key].IsArray)
            {
                arrayNodeName = key;
                arrayNode = obj[key].AsArray;
                break;
            }
        }
    
        if (arrayNode == null || arrayNode.Count == 0)
        {
            LogUtility.Warning(LogLayer.Framework, "XmlReader", "表格格式XML中没有找到数组数据");
            return configData;
        }
        
        // 从第一行获取列名
        var firstRow = arrayNode[0];
        if (!firstRow.IsObject)
        {
            LogUtility.Error(LogLayer.Framework, "XmlReader", "表格格式XML中的行应为对象格式");
            return configData;
        }
    
        List<string> columns = new List<string>();
        var firstRowObj = firstRow.AsObject;
    
        foreach (var key in firstRowObj.Keys)
        {
            if (!key.StartsWith("@")) // 忽略属性
                columns.Add(key);
        }
    
        configData.Columns = columns.ToArray();
    
        // 处理每一行数据
        for (int i = 0; i < arrayNode.Count; i++)
        {
            var rowNode = arrayNode[i];
            if (!rowNode.IsObject)
            {
                LogUtility.Warning(LogLayer.Framework, "XmlReader", $"第 {i} 行不是对象格式，跳过");
                continue;
            }
        
            var rowObj = rowNode.AsObject;
            object[] row = new object[columns.Count];
        
            for (int j = 0; j < columns.Count; j++)
            {
                string columnName = columns[j];
                if (rowObj.HasKey(columnName))
                {
                    var fieldValue = rowObj[columnName];
                    row[j] = ConvertXmlValueToObject(fieldValue);
                }
                else
                {
                    row[j] = null;
                    LogUtility.Warning(LogLayer.Framework, "XmlReader", $"字段 '{columnName}' 在第 {i} 行中不存在");
                }
            }
        
            rows.Add(row);
        }
    
        configData.Rows = rows;
        return configData;
    }

    /// <summary>
    /// 解析键值对格式的XML文件
    /// 示例格式:
    /// <root>
    ///   <player1><id>1</id><name>A</name></player1>
    ///   <player2><id>2</id><name>B</name></player2>
    /// </root>
    /// </summary>
    private ConfigData ParseKeyValueFormat(XMLNode rootNode, ConfigData configData)
    {
        var rows = new List<object[]>();
        var obj = rootNode.AsObject;
    
        // 获取所有对象类型的子节点（忽略属性）
        List<string> objectKeys = new List<string>();
        foreach (var key in obj.Keys)
        {
            if (!key.StartsWith("@") && obj[key].IsObject)
            {
                objectKeys.Add(key);
            }
        }
    
        if (objectKeys.Count == 0)
        {
            LogUtility.Warning(LogLayer.Framework, "XmlReader", "键值对格式XML中没有找到对象数据");
            return configData;
        }
    
        // 收集所有可能的列名（从所有对象中）
        HashSet<string> allColumns = new HashSet<string> {"id"};
    
        foreach (var key in objectKeys)
        {
            var valueObj = obj[key].AsObject;
            foreach (var fieldKey in valueObj.Keys)
            {
                if (!fieldKey.StartsWith("@"))
                {
                    allColumns.Add(fieldKey);
                }
            }
        }
    
        List<string> columns = allColumns.ToList();
        configData.Columns = columns.ToArray();
    
        // 处理每个对象
        foreach (var key in objectKeys)
        {
            var value = obj[key];
            if (!value.IsObject)
            {
                LogUtility.Warning(LogLayer.Framework, "XmlReader", $"键 '{key}' 的值不是对象格式，跳过");
                continue;
            }
        
            var valueObj = value.AsObject;
            object[] row = new object[columns.Count];
        
            // 设置ID
            int idIndex = columns.IndexOf("id");
            if (idIndex >= 0)
            {
                row[idIndex] = key;
            }
        
            // 设置其他字段
            for (int i = 0; i < columns.Count; i++)
            {
                string columnName = columns[i];
                if (columnName == "id") continue; // 已经处理过ID
            
                if (valueObj.HasKey(columnName))
                {
                    var fieldValue = valueObj[columnName];
                    row[i] = ConvertXmlValueToObject(fieldValue);
                }
                else
                {
                    row[i] = null;
                }
            }
        
            rows.Add(row);
        }
    
        configData.Rows = rows;
        return configData;
    }
    
    /// <summary>
    /// 将XML值转换为合适的C#对象
    /// </summary>
    private object ConvertXmlValueToObject(XMLNode value)
    {
        if (value.IsNull)
            return null;
        else if (value.IsString)
            return value.Value;
        else if (value.IsNumber)
            return value.AsDouble;
        else if (value.IsBoolean)
            return value.AsBool;
        else
            return value.Value; // 默认转为字符串
    }

    #endregion
}