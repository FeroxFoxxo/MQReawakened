using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.LocalAssets;

public static class AssetDictionaryExtensions
{
    public static void AddModifiedAssets(this Dictionary<string, InternalAssetInfo> assets,
        AssetBundleStaticConfig config)
    {
        var assetsToAdd = new Dictionary<string, InternalAssetInfo>();

        foreach (var modifier in config.AssetModifiers)
        {
            foreach (var oldAsset in assets.Keys.Where(a => a.EndsWith(modifier)))
            {
                var assetName = oldAsset[..^modifier.Length];
                assetsToAdd.AddChangedNameToDict(assetName, assets[oldAsset]);
            }
        }

        foreach (var replacement in config.AssetRenames)
        {
            foreach (var oldAsset in assets.Keys.Where(a => a.Contains(replacement.Key)))
            {
                var assetName = oldAsset.Replace(replacement.Key, replacement.Value);
                assetsToAdd.AddChangedNameToDict(assetName, assets[oldAsset]);
            }
        }

        foreach (var asset in assetsToAdd
                     .Where(asset => !assets.ContainsKey(asset.Key)))
            assets.Add(asset.Key, asset.Value);
    }

    public static void AddChangedNameToDict(this Dictionary<string, InternalAssetInfo> assets, string assetName,
        InternalAssetInfo oldAsset)
    {
        if (assets.ContainsKey(assetName))
            return;

        var asset = oldAsset.DeepCopy();
        asset.Name = assetName;

        assets.Add(assetName, asset);
    }

    public static void AddLocalXmlFiles(this Dictionary<string, InternalAssetInfo> assets,
        ILogger<BuildAssetList> logger, AssetBundleStaticConfig config)
    {
        var localPath = Path.Combine(InternalDirectory.GetBaseDirectory(), "LocalAssets");

        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        foreach (var asset in Directory
                     .GetFiles(localPath, "*.xml")
                     .Select(file => new InternalAssetInfo
                     {
                         BundleSize = Convert.ToInt32(new FileInfo(file).Length / 1024),
                         Locale = RFC1766Locales.LanguageCodes.en_us,
                         Name = Path.GetFileName(file).Split('.')[0],
                         Type = AssetInfo.TypeAsset.XML,
                         Path = file,
                         Version = 0
                     })
                     .Where(a =>
                     {
                         if (assets.ContainsKey(a.Name))
                         {
                             if (!config.ForceLocalAsset.Contains(a.Name))
                                 return false;
                             assets.Remove(a.Name);
                         }

                         logger.LogTrace("Adding asset {Name} from local assets.", a.Name);
                         return true;
                     }))

            assets.Add(asset.Name, asset);
    }
}
