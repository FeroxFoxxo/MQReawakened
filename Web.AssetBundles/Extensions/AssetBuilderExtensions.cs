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
            .SelectMany(g => g.OrderBy(x => x.Name).ToArray());

    public static Dictionary<string, InternalAssetInfo> GetClosestBundles(this IEnumerable<InternalAssetInfo> assets,
        StartConfig config)
    {
        var filteredAssets = new Dictionary<string, InternalAssetInfo>();

        foreach (var newAsset in assets)
        {
            if (!filteredAssets.ContainsKey(newAsset.Name))
            {
                filteredAssets.Add(newAsset.Name, newAsset);
            }
            else
            {
                var oldAssetTime = filteredAssets[newAsset.Name].CacheTime - config.LastClientUpdate;
                var newAssetTime = newAsset.CacheTime - config.LastClientUpdate;

                if (config.Is2014Client)
                {
                    if (newAssetTime > oldAssetTime)
                        filteredAssets[newAsset.Name] = newAsset;
                }
                else
                {
                    if (newAssetTime < oldAssetTime)
                        filteredAssets[newAsset.Name] = newAsset;
                }
            }
        }

        return filteredAssets;
    }
}
