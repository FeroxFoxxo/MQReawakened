using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.LocalAssets;

public static class AssetDictionaryExtensions
{
    public static void AddModifiedAssets(this Dictionary<string, InternalAssetInfo> assets, AssetBundleConfig config)
    {
        var assetsToAdd = new Dictionary<string, InternalAssetInfo>();

        foreach (var modifier in config.AssetModifiers)
        {
            foreach (var storedAsset in assets.Keys.Where(a => a.EndsWith(modifier)))
            {
                var assetName = storedAsset[..^modifier.Length];

                if (assets.ContainsKey(assetName))
                    continue;

                var asset = assets[storedAsset].DeepCopy();
                asset.Name = assetName;

                assetsToAdd.Add(assetName, asset);
            }
        }

        foreach (var asset in assetsToAdd)
            assets.Add(asset.Key, asset.Value);
    }

    public static void AddLocalXmlFiles(this Dictionary<string, InternalAssetInfo> assets,
        ILogger<BuildAssetList> logger, AssetBundleConfig config)
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
