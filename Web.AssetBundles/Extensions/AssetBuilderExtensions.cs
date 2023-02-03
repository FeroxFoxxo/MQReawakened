using System.Globalization;
using Web.AssetBundles.Models;
using Web.Launcher.Models;

namespace Web.AssetBundles.Extensions;

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
            .SelectMany(g => g.OrderBy(x => x.Name).ToList());

    public static Dictionary<string, InternalAssetInfo> GetClosestBundles(this IEnumerable<InternalAssetInfo> assets,
        LauncherConfig config)
    {
        var filteredAssets = new Dictionary<string, InternalAssetInfo>();

        var oldTime = (long)DateTime
            .ParseExact(config.OldClientLastUpdate, config.TimeFilter, CultureInfo.InvariantCulture)
            .ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;

        foreach (var newAsset in assets)
        {
            if (!filteredAssets.ContainsKey(newAsset.Name))
            {
                filteredAssets.Add(newAsset.Name, newAsset);
            }
            else
            {
                var oldAsset = filteredAssets[newAsset.Name];

                if (oldAsset.CacheTime > oldTime)
                {
                    if (oldAsset.CacheTime > newAsset.CacheTime)
                        filteredAssets[newAsset.Name] = newAsset;
                }
                else if (oldAsset.CacheTime < newAsset.CacheTime)
                {
                    filteredAssets[newAsset.Name] = newAsset;
                }

                filteredAssets[newAsset.Name] = newAsset;
            }
        }

        return filteredAssets;
    }
}
