
/// <summary>
/// 配置文件读取接口
/// </summary>
public interface IConfigReader
{
    ConfigData Read(string filePath);
    ConfigFormat SupportedFormat { get; } // 添加支持的格式属性
}
