using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class HotfixManager
{
    public async static Task InitializeAsync()
    {
        // 1. 初始化 Addressable 本地包
        
        // 2. AAPackageManager 基于本地索引扫描构建
        
        // 3. 下载远端更新包
        
        // 4. 版本比对
        
        // 若旧版本包有更新
        if (1 != 0)
        {
            // 5. 合并catalog
            
            // 6. 删除旧版本包
            
            // 7. 构建RemoteIndex （只存非常小的key映射）
        }
        
        // 此时正式开放AAPackageManager 的获取资源功能API
    }
}
