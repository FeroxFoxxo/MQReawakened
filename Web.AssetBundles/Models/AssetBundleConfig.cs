using Server.Base.Core.Abstractions;

namespace Web.AssetBundles.Models;

public class AssetBundleConfig : IConfig
{
    public string CacheInfoFile { get; set; }
    public string WebPlayerInfoFile { get; set; }
    public bool FlushCacheOnStart { get; set; }

    public AssetBundleConfig()
    {
        FlushCacheOnStart = true;
        WebPlayerInfoFile = string.Empty;
        CacheInfoFile = string.Empty;
    }
}
