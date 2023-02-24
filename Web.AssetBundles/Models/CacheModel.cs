using Web.AssetBundles.Services;

namespace Web.AssetBundles.Models;

public class CacheModel
{
    private readonly Dictionary<string, InternalAssetInfo> _assetDictionary;
    public int TotalAssetDictionaryFiles { get; set; }
    public int TotalCachedAssetFiles { get; set; }
    public int TotalFoundCaches { get; set; }
    public int TotalUnknownCaches { get; set; }

    public Dictionary<string, List<string>> FoundCaches { get; set; }
    public Dictionary<string, string> UnknownCaches { get; set; }

    public CacheModel(BuildAssetList buildAssetList, AssetBundleRwConfig rwConfig)
    {
        FoundCaches = new Dictionary<string, List<string>>();
        UnknownCaches = new Dictionary<string, string>();

        _assetDictionary = buildAssetList.InternalAssets.Values
            .Select(a => new KeyValuePair<string, InternalAssetInfo>(Path.GetFileName(a.Path), a))
            .DistinctBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value);

        var caches = Directory.GetFiles(Path.GetDirectoryName(rwConfig.WebPlayerInfoFile)!, "*.*",
                SearchOption.AllDirectories).Select(c => new KeyValuePair<string, string>(Path.GetFileName(c), c))
            .Where(c => c.Key != "__info").ToArray();

        foreach (var cache in caches)
        {
            if (_assetDictionary.ContainsKey(cache.Key!))
            {
                if (!FoundCaches.ContainsKey(cache.Key))
                    FoundCaches.Add(cache.Key, new List<string>());

                FoundCaches[cache.Key].Add(cache.Value);
            }
            else
            {
                UnknownCaches.Add(cache.Key, cache.Value);
            }
        }

        TotalAssetDictionaryFiles = _assetDictionary.Count;
        TotalCachedAssetFiles = caches.Length;

        TotalFoundCaches = FoundCaches.Count;
        TotalUnknownCaches = UnknownCaches.Count;
    }

    public InternalAssetInfo GetAssetInfoFromCacheName(string name) =>
        _assetDictionary[name];
}
