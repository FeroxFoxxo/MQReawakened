using Web.AssetBundles.Models;

namespace Web.AssetBundles.Events.Arguments;

public class AssetBundleLoadEventArgs
{
    public readonly Dictionary<string, InternalAssetInfo> InternalAssets;

    public AssetBundleLoadEventArgs(Dictionary<string, InternalAssetInfo> internalAssets) =>
        InternalAssets = internalAssets;
}
