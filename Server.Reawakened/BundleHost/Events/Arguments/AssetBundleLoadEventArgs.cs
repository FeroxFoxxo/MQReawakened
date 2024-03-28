using Server.Reawakened.BundleHost.Models;

namespace Server.Reawakened.BundleHost.Events.Arguments;

public class AssetBundleLoadEventArgs(Dictionary<string, InternalAssetInfo> internalAssets)
{
    public readonly Dictionary<string, InternalAssetInfo> InternalAssets = internalAssets;
}
