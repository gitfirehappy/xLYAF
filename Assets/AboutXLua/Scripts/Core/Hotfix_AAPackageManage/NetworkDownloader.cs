using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkDownloader : Singleton<NetworkDownloader>
{
    public async Task DownLoadRemotePackages()
    {
        // 对接 Netlify / CDN / 任意服务器
        // 下载版本文件、catalog、bundle 等文件
    }
}
