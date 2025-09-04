using System.Collections.Generic;
using System.IO;
using System.Text;

public class CsvReader : IConfigReader
{
    public ConfigFormat SupportedFormat => ConfigFormat.Csv;
    
    public ConfigData Read(string filePath)
    {
        var configData = new ConfigData();
        var rows = new List<object[]>();

        // 设置原始格式类型
        configData.PrimitiveFormat = ConfigFormat.Csv;
        
        // 读取CSV文件所有行
        string[] allLines = File.ReadAllLines(filePath, Encoding.UTF8);

        if (allLines.Length == 0)
            return configData;

        // 第一行为列名（Columns）
        configData.Columns = ParseCsvLine(allLines[0]);

        // 从第二行开始解析数据行（Rows）
        for (int i = 1; i < allLines.Length; i++)
        {
            if (string.IsNullOrEmpty(allLines[i]))
                continue;

            object[] rowValues = ParseCsvLine(allLines[i]);
            rows.Add(rowValues);
        }

        configData.Rows = rows;
        return configData;
    }

    /// <summary>
    /// 解析CSV单行（处理逗号分隔和引号转义）
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // 处理双引号转义（""表示一个引号）
                if (inQuotes && i < line.Length - 1 && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++; // 跳过下一个引号
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        values.Add(current.ToString()); // 添加最后一个字段
        return values.ToArray();
    }
}