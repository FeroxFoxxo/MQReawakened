using Server.Base.Core.Abstractions;

namespace Server.Reawakened.BundleHost.Configs;

public class AssetBundleRwConfig : IRwConfig
{
    public string CacheInfoFile { get; set; }
    public string WebPlayerInfoFile { get; set; }
    public bool FlushCacheOnStart { get; set; }
    public bool LogProgressBars { get; set; }
    public long LastDecompiledScriptUpdate { get; set; }
    public string DatabaseDirectory { get; set; }
    public string DatabaseXMLDirectory { get; set; }
    public string DatabaseLevelDirectory { get; set; }
    public bool UseCustomAssetLoader { get; set; }

    public AssetBundleRwConfig()
    {
        FlushCacheOnStart = true;
        WebPlayerInfoFile = string.Empty;
        CacheInfoFile = string.Empty;
        LogProgressBars = false;
        LastDecompiledScriptUpdate = 0;
        DatabaseDirectory = string.Empty;
        DatabaseXMLDirectory = string.Empty;
        DatabaseLevelDirectory = string.Empty;
        UseCustomAssetLoader = false;
    }
}
