using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigConverter : Singleton<ConfigConverter>
{
    private Dictionary<ConfigFormat, IConfigReader> _readers;
    private Dictionary<ConfigFormat, IConfigWriter> _writers;
    
    private ConfigConvertSettings _configConvertSettings;

    #region 外部方法
    
    /// <summary>
    /// 初始化所有读取器和写入器
    /// </summary>
    private void InitializeConverters()
    {
        _readers = new Dictionary<ConfigFormat, IConfigReader>
        {
            { ConfigFormat.Csv, new CsvReader() }
            // 可以添加其他读取器
        };
        
        _writers = new Dictionary<ConfigFormat, IConfigWriter>
        {
            { ConfigFormat.Lua, new LuaWriter() }
            // 可以添加其他写入器
        };
    }
    
    /// <summary>
    /// 转换单个文件(运行时主要调用)
    /// </summary>
    /// <param name="inputFile">输入文件</param>
    /// <param name="channel"></param>
    /// <returns></returns>
    public bool ConvertFile(string inputFile, ConfigConvertChannel channel)
    {
        if (!File.Exists(inputFile))
        {
            LogUtility.Error(LogLayer.Framework, "ConfigConverter", 
                $"输入文件不存在: {inputFile}");
            return false;
        }
        
        if (!ValidateChannel(channel))
        {
            LogUtility.Error(LogLayer.Framework, "ConfigConverter", 
                $"通道配置验证失败: {channel.name}");
            return false;
        }
        
        try
        {
            // 获取文件名（不含扩展名）
            string fileName = Path.GetFileNameWithoutExtension(inputFile);
            
            // 构建输出路径
            string outputFilePath = Path.Combine(
                channel.outputFolder, 
                $"{fileName}.{channel.outputFormat.ToString().ToLower()}");
            
            // 确保输出目录存在
            string outputDir = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                LogUtility.Info(LogLayer.Framework, "ConfigConverter",
                    $"输出目录不存在，已创建: {outputDir}");
            }
            
            // 读取文件
            if (!_readers.TryGetValue(channel.inputFormat, out IConfigReader reader))
            {
                LogUtility.Error(LogLayer.Framework, "ConfigConverter",
                    $"找不到 {channel.inputFormat} 格式的读取器");
                return false;
            }
            
            ConfigData data = reader.Read(inputFile);
            
            // 写入文件
            if (!_writers.TryGetValue(channel.outputFormat, out IConfigWriter writer))
            {
                LogUtility.Error(LogLayer.Framework, "ConfigConverter",
                    $"找不到 {channel.outputFormat} 格式的写入器");
                return false;
            }
            
            // 使用默认选项写入
            writer.Write(outputFilePath, data);
            
            LogUtility.Info(LogLayer.Framework, "ConfigConverter",
                $"转换成功: {inputFile} -> {outputFilePath}");
            return true;
        }
        catch (System.Exception ex)
        {
            LogUtility.Error(LogLayer.Framework, "ConfigConverter",
                $"转换文件 {inputFile} 时出错: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }
    
    /// <summary>
    /// 进行配置转换(单个通道)
    /// </summary>
    public void Convert(ConfigConvertChannel channel)
    {
        if (!ValidateChannel(channel)) return;
        
        // 获取输入目录中的所有匹配文件
        string searchPattern = GetFilePattern(channel.inputFormat);
        string[] inputFiles = Directory.GetFiles(channel.inputFolder, searchPattern);
        
        if (inputFiles.Length == 0)
        {
            LogUtility.Warning(LogLayer.Framework,"ConfigConverter",
                $"在目录 {channel.inputFolder} 中没有找到 {channel.inputFormat} 文件");
            return;
        }
        
        int successCount = 0;
        foreach (string inputFile in inputFiles)
        {
            if (ConvertFile(inputFile, channel))
            {
                successCount++;
            }
        }
        
        LogUtility.Info(LogLayer.Framework,"ConfigConverter",
            $"通道: {channel.name} 转换完成: {successCount}/{inputFiles.Length} 个文件成功转换");
    }

    /// <summary>
    /// 进行所有通道配置转换(尽量不要在运行时调用)
    /// </summary>
    public void ConvertAll(ConfigConvertSettings settings)
    {
        if (settings == null || settings.channels == null || settings.channels.Count == 0)
        {
            LogUtility.Warning(LogLayer.Framework, "ConfigConverter",
                "没有配置转换通道，请检查ConfigConvertSettings");
            return;
        }
        
        int totalChannels = settings.channels.Count;
        int completedChannels = 0;
        
        foreach (var channel in settings.channels)
        {
            LogUtility.Info(LogLayer.Framework, "ConfigConverter",
                $"开始转换通道: {channel.name} ({completedChannels + 1}/{totalChannels})");
            
            Convert(channel);
            completedChannels++;
        }
        
        LogUtility.Info(LogLayer.Framework, "ConfigConverter",
            $"所有通道转换完成: {completedChannels}/{totalChannels} 个通道成功处理");
    }
    
    #endregion

    #region 内部方法
    
    /// <summary>
    /// 验证通道配置是否有效
    /// </summary>
    private bool ValidateChannel(ConfigConvertChannel channel)
    {
        if (string.IsNullOrEmpty(channel.inputFolder) || !Directory.Exists(channel.inputFolder))
        {
            LogUtility.Error(LogLayer.Framework,"ConfigConverter",
                $"通道 '{channel.name}' 的输入目录不存在: {channel.inputFolder}");
            return false;
        }
        
        if (string.IsNullOrEmpty(channel.outputFolder))
        {
            LogUtility.Error(LogLayer.Framework,"ConfigConverter",
                $"通道 '{channel.name}' 的输出目录未设置");
            return false;
        }
        
        if (!_readers.ContainsKey(channel.inputFormat))
        {
            LogUtility.Error(LogLayer.Framework,"ConfigConverter",
                $"通道 '{channel.name}' 的输入格式 {channel.inputFormat} 不受支持");
            return false;
        }
        
        if (!_writers.ContainsKey(channel.outputFormat))
        {
            LogUtility.Error(LogLayer.Framework,"ConfigConverter",
                $"通道 '{channel.name}' 的输出格式 {channel.outputFormat} 不受支持");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 根据格式获取文件搜索模式
    /// </summary>
    private string GetFilePattern(ConfigFormat format)
    {
        switch (format)
        {
            case ConfigFormat.Csv:
                return "*.csv";
            case ConfigFormat.Lua:
                return "*.lua";
            case ConfigFormat.Json:
                return "*.json";
            case ConfigFormat.Xml:
                return "*.xml";
            default:
                return "*.*";
        }
    }
    
    #endregion
}
