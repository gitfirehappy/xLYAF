
/// <summary>
/// 配置文件写入接口
/// </summary>
public interface IConfigWriter
{
    void Write(string outPutPath, ConfigData data, WriterOptions options = null);
}
