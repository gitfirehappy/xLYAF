using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// MD5哈希生成器
/// </summary>
public static class HashGenerator
{
    /// <summary>
    /// 生成字符串的MD5哈希
    /// </summary>
    public static string GenerateStringHash(string content)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    
    /// <summary>
    /// 生成单个文件的MD5哈希
    /// </summary>
    public static string GenerateFileHash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }
    
    /// <summary>
    /// 生成热更新包的MD5
    /// </summary>
    /// <param name="hotfixDir">热更包的路径</param>
    public static string GeneratePackageHash(string hotfixDir)
    {
        var hashList = new StringBuilder();
        
        // 计算整个热更新包目录的MD5（跳过version_state.json自身）
        foreach (var file in Directory.GetFiles(hotfixDir, "*", SearchOption.AllDirectories))
        {
            if (Path.GetFileName(file) == "version_state.json") continue;
            
            hashList.Append(GenerateFileHash(file));
        }
        
        // 对所有文件hash拼接后计算最终MD5
        using (var finalMd5 = MD5.Create())
        {
            return BitConverter.ToString(finalMd5.ComputeHash(Encoding.UTF8.GetBytes(hashList.ToString())))
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}