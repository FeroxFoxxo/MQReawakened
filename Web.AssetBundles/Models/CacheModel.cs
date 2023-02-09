using Web.AssetBundles.Services;

namespace Web.AssetBundles.Models;

public class CacheModel
{
    public int TotalAssetDictionaryFiles { get; set; }
    public int TotalCachedAssetFiles { get; set; }
    public int TotalFoundCaches { get; set; }
    public int TotalUnknownCaches { get; set; }

    public Dictionary<string, string> FoundCaches { get; set; }
    public Dictionary<string, string> UnknownCaches { get; set; }

    private readonly Dictionary<string, InternalAssetInfo> assetDictionary;

    public CacheModel(BuildAssetList buildAssetList, AssetBundleConfig config)
    {
        FoundCaches = new Dictionary<string, string>();
        UnknownCaches = new Dictionary<string, string>();

        assetDictionary = buildAssetList.InternalAssets.Values
            .Select(a => new KeyValuePair<string, InternalAssetInfo>(Path.GetFileName(a.Path), a))
        .DistinctBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value);

        var caches = Directory.GetFiles(Path.GetDirectoryName(config.WebPlayerInfoFile)!, "*.*",
                SearchOption.AllDirectories).Select(c => new KeyValuePair<string, string>(Path.GetFileName(c), c))
            .Where(c => c.Key != "__info").ToArray();
        
        foreach (var cache in caches)
        {
            if (assetDictionary.ContainsKey(cache.Key!))
                FoundCaches.Add(cache.Key, cache.Value);
            else
                UnknownCaches.Add(cache.Key, cache.Value);
        }

        TotalAssetDictionaryFiles = assetDictionary.Count;
        TotalCachedAssetFiles = caches.Length;

        TotalFoundCaches = FoundCaches.Count;
        TotalUnknownCaches = UnknownCaches.Count;
    }

    public InternalAssetInfo GetAssetInfoFromCacheName(string name) =>
        assetDictionary[name];
}
