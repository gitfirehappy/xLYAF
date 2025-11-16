using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteIndex
{
    private HashSet<string> _remoteKeys = new();
    public bool IsRemote(string key) => _remoteKeys.Contains(key);

    public static RemoteIndex Build(Dictionary<string, PackageEntry> remoteCatalog)
    {
        var index = new RemoteIndex();
        foreach (var kv in remoteCatalog)
        {
            index._remoteKeys.Add(kv.Key);
        }
        return index;
    }
}
