using System.Collections.Generic;

/// <summary>
/// 配置文件中间态数据
/// </summary>
public class ConfigData
{
    public ConfigFormat PrimitiveFormat;
    
    // 字段名列表（确定列顺序，如["id","name","level"]）
    public string[] Columns;
    
    // 行数据集合（每行是一个值数组，索引对应Columns的索引）
    // 例如：[[1,"玩家1",30], [2,"玩家2",25]]
    public List<object[]> Rows;
}