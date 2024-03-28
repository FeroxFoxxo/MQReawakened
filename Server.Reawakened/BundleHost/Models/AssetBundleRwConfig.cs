using Server.Base.Core.Abstractions;

namespace Server.Reawakened.BundleHost.Models;

public class AssetBundleRwConfig : IRwConfig
{
    public string CacheInfoFile { get; set; }
    public string WebPlayerInfoFile { get; set; }
    public bool FlushCacheOnStart { get; set; }
    public bool LogProgressBars { get; set; }

    public AssetBundleRwConfig()
    {
        FlushCacheOnStart = true;
        WebPlayerInfoFile = string.Empty;
        CacheInfoFile = string.Empty;
        LogProgressBars = false;
    }
}
