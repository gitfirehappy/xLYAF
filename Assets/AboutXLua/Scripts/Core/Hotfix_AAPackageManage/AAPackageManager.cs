using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AAPackageManager : Singleton<AAPackageManager>
{
    private Dictionary<string, PackageEntry> _dataDomain = new();
    private RemoteIndex _remoteIndex;

    public void Initialize(RemoteIndex remoteIndex)
    {
        _remoteIndex = remoteIndex;
        ScanLocalPackages();
    }
    
    private void ScanLocalPackages()
    { 
        // 扫描初始的AA包，根据 label/type/key 分类
        // 构建 DataDomain
        // 只关心 key，不关心路径（路径后续热更会变化）
    }
    
    public PackageEntry GetPackageEntry(string key)
    {
        if (_remoteIndex != null && _remoteIndex.IsRemote(key))
        {
            return _dataDomain[key].UseRemote();
        }
        return _dataDomain[key].UseLocal();
    }
    
    public void UpdateRemoteCatalog(Dictionary<string, PackageEntry> merged)
    {
        
    }
}

public class PackageEntry
{
    public string key;
    public string Type;
    public string Label;

    public string LocalPath;
    public string RemotePath;
    
    public PackageEntry UseRemote()
    { 
        // 返回信息
        return this;
    }
    
    public PackageEntry UseLocal()
    {
        return this;
    }
}
