using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.LocalAssets;

public static class AssetExtensionsLocal
{
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
