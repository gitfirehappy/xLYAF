using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 对接 Netlify / CDN / 任意服务器
/// 下载版本文件、catalog、bundle 等文件
/// </summary>
public class NetworkDownloader : Singleton<NetworkDownloader>
{
    public event Action<float> OnProgress; 
    
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="savePath">由PathManager决定</param>
    /// <returns></returns>
    public async Task<bool> DownloadFile(string url, string savePath)
    {
        using var uwr = UnityWebRequest.Get(url);

        uwr.downloadHandler = new DownloadHandlerFile(savePath);
        var operation = uwr.SendWebRequest();

        while (!operation.isDone)
        {
            OnProgress?.Invoke(operation.progress);
            await Task.Yield();
        }

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[NetworkDownloader] 下载文件失败: {url}\n{uwr.error}");
            return false;
        }

        Debug.Log($"[NetworkDownloader] 下载文件成功: {url}");
        return true;
    }

    public async Task<string> DownloadText(string url)
    {
        using var uwr = UnityWebRequest.Get(url);
        var operation = uwr.SendWebRequest();

        while (!operation.isDone)
        {
            OnProgress?.Invoke(operation.progress);
            await Task.Yield();
        }

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[NetworkDownloader] 下载文本失败: {url}\n{uwr.error}");
            return null;
        }

        return uwr.downloadHandler.text;
    }

    public async Task<byte[]> DownloadBytes(string url)
    {
        using var uwr = UnityWebRequest.Get(url);
        var operation = uwr.SendWebRequest();

        while (!operation.isDone)
        {
            OnProgress?.Invoke(operation.progress);
            await Task.Yield();
        }

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[NetworkDownloader] 下载字节失败: {url}\n{uwr.error}");
            return null;
        }

        return uwr.downloadHandler.data;
    }
}
