using Web.AssetBundles.Models;

namespace Web.AssetBundles.Events.Arguments;

public class AssetBundleLoadEventArgs(Dictionary<string, InternalAssetInfo> internalAssets)
{
    public readonly Dictionary<string, InternalAssetInfo> InternalAssets = internalAssets;
}
