using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class XmlReader : IConfigReader
{
    public ConfigFormat SupportedFormat => ConfigFormat.Xml;

    public ConfigData Read(string filePath)
    {
        var configData = new ConfigData();
        configData.PrimitiveFormat = ConfigFormat.Xml;

        try
        {
            // 加载XML文档
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            // 获取根节点
            XmlNode rootNode = xmlDoc.DocumentElement;

            // 检查XML格式类型
            if (IsTableFormat(rootNode))
            {
                return ParseTableFormat(xmlDoc, configData);
            }
            else if (IsKeyValueFormat(rootNode))
            {
                return ParseKeyValueFormat(xmlDoc, configData);
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
    private bool IsTableFormat(XmlNode rootNode)
    {
        return true;
    }

    /// <summary>
    /// 检查XML格式是否为键值对格式
    /// </summary>
    private bool IsKeyValueFormat(XmlNode rootNode)
    {
        return true;
    }

    #endregion

    #region 解析XML

    /// <summary>
    /// 解析表格格式的XML文件
    /// </summary>
    private ConfigData ParseTableFormat(XmlDocument xmlDoc, ConfigData configData)
    {
        return configData;
    }

    /// <summary>
    /// 解析键值对格式的XML文件
    /// </summary>
    private ConfigData ParseKeyValueFormat(XmlDocument xmlDoc, ConfigData configData)
    {
        return configData;
    }

    #endregion
}