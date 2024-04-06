using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;

namespace Server.Reawakened.BundleHost.Extensions;

public static class AssetBuilderExtensions
{
    public static List<string> GetLowestDirectories(this string directory)
    {
        var list = new List<string>();
        GetLowestDirectories(directory, list);
        return list;
    }

    private static void GetLowestDirectories(string directory, ICollection<string> directories)
    {
        var subDirs = Directory.GetDirectories(directory);

        if (subDirs.Length > 0)
            foreach (var subDir in subDirs)
                GetLowestDirectories(subDir, directories);
        else
            directories.Add(directory);
    }

    public static IEnumerable<InternalAssetInfo> OrderAssets(this IEnumerable<InternalAssetInfo> assets) =>
        assets.GroupBy(x => x.Type)
            .SelectMany(g => g.OrderBy(x => x.Name).ToArray());

    public static Dictionary<string, InternalAssetInfo> GetClosestBundles(this IEnumerable<InternalAssetInfo> assets, ServerRConfig sConfig)
    {
        var filteredAssets = new Dictionary<string, InternalAssetInfo>();

        foreach (var newAsset in assets)
            if (!filteredAssets.TryGetValue(newAsset.Name, out var value))
            {
                filteredAssets.Add(newAsset.Name, newAsset);
            }
            else
            {
                var oldAssetTime = value.CacheTime - sConfig.CutOffFor2014;
                var newAssetTime = newAsset.CacheTime - sConfig.CutOffFor2014;

                if (sConfig.GameVersion <= GameVersion.vLate2013)
                {
                    oldAssetTime *= -1;
                    newAssetTime *= -1;
                }

                if (newAssetTime >= 0)
                {
                    if (oldAssetTime >= 0)
                    {
                        var oldAdjusted = Math.Abs(value.CacheTime - sConfig.LastClientUpdate);
                        var newAdjusted = Math.Abs(newAsset.CacheTime - sConfig.LastClientUpdate);

                        if (newAdjusted < oldAdjusted)
                            filteredAssets[newAsset.Name] = newAsset;
                    }
                    else
                        filteredAssets[newAsset.Name] = newAsset;
                }
                else if (oldAssetTime < 0 && newAssetTime > oldAssetTime)
                    filteredAssets[newAsset.Name] = newAsset;
            }

        return filteredAssets;
    }
}
