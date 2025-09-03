
/// <summary>
/// 配置文件读取接口
/// </summary>
public interface IConfigReader
{
    ConfigData Read(string filePath);
}
