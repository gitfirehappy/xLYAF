using System.Collections.Generic;

/// <summary>
/// 配置文件中间态数据
/// </summary>
public class ConfigData
{
    public ConfigFormat PrimitiveFormat { get; set;}

    public ConfigMode Mode { get; set;}
    
    // 字段名列表（确定列顺序，如["id","name","level"]）
    public string[] Columns { get; set; }
    
    // 行数据集合（每行是一个值数组，索引对应Columns的索引）
    public List<object[]> Rows { get; set; }
}