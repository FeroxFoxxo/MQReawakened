using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;

namespace Server.Reawakened.BundleHost.Services;

public class RemoveDuplicates(ILogger<RemoveDuplicates> logger, EventSink sink,
    ServerConsole console, ServerRConfig config, BuildAssetList buildAssetList,
    AssetBundleRConfig rConfig, AssetBundleRwConfig rwConfig) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load()
    {
        console.AddCommand(
            "removeDuplicates",
            "Creates a directory that does not include duplicated caches.",
            NetworkType.Server | NetworkType.Client,
            _ => RemoveDuplicateFiles()
        );
        console.AddCommand(
            "removeXmlDuplicates",
            "Creates a directory that does not include duplicated XML files.",
            NetworkType.Server | NetworkType.Client,
            _ => RemoveDuplicateFiles([AssetInfo.TypeAsset.Level, AssetInfo.TypeAsset.XML])
        );
    }

    private void RemoveDuplicateFiles(AssetInfo.TypeAsset[] filters = null)
    {
        logger.LogDebug("Removing duplicates");

        var assetList = new Dictionary<string, List<InternalAssetInfo>>();
        var assetDict = File.ReadAllText(buildAssetList.AssetDictLocation);
        var allAssets = BuildAssetList.GetAssetsFromDictionary(assetDict)
            .Where(x => filters?.Any(f => x.Type == f) ?? true)
            .ToArray();

        using (
            var bar = new DefaultProgressBar(
                allAssets.Length,
                "Reading assets from disk",
                logger,
                rwConfig
            )
        )
            foreach (var asset in allAssets)
            {
                var assetName = asset.Name.Trim().ToLower();

                if (!assetList.ContainsKey(assetName))
                    assetList.Add(assetName, []);

                var hasFoundExisting = false;

                foreach (var containedAsset in assetList[assetName]
                             .Where(containedAsset =>
                                 containedAsset.BundleSize == asset.BundleSize &&
                                 containedAsset.UnityVersion == asset.UnityVersion &&
                                 containedAsset.Locale == asset.Locale &&
                                 containedAsset.Type == asset.Type &&
                                 Path.GetFileName(containedAsset.Path) == Path.GetFileName(asset.Path)
                             )
                        )
                {
                    if (!AreFileContentsEqual(containedAsset.Path, asset.Path))
                        continue;

                    if (containedAsset.CacheTime > asset.CacheTime && config.GameVersion >= GameVersion.vEarly2014 ||
                        containedAsset.CacheTime < asset.CacheTime && config.GameVersion <= GameVersion.vLate2013)
                        assetList[assetName].Remove(containedAsset);
                    else
                        hasFoundExisting = true;

                    break;
                }

                if (!hasFoundExisting)
                    assetList[assetName].Add(asset);

                bar.TickBar();
            }

        var replacedCount = assetList.Sum(s => s.Value.Count);

        logger.LogInformation("Removed {Count} duplicates, asset count: {Total} (of {OldTotal})",
            allAssets.Length - replacedCount, replacedCount, allAssets.Length);

        logger.LogDebug("Writing assets");

        logger.LogDebug("Emptying duplicated bundle directory...");
        InternalDirectory.Empty(rConfig.RemovedDuplicateDirectory);
        logger.LogDebug("Emptied folder");

        var totalDirectories = assetList.Max(s => s.Value?.Count ?? 0);

        using (
            var bar = new DefaultProgressBar(
                replacedCount + totalDirectories,
                "Writing Assets To Disk",
                logger,
                rwConfig
            )
        )
            foreach (var assets in assetList)
                for (var i = 0; i < assets.Value.Count; i++)
                {
                    var targetDirectory = Path.Combine(rConfig.RemovedDuplicateDirectory, assets.Key, i.ToString());
                    InternalDirectory.CreateDirectory(targetDirectory);

                    var sourceDirectory = Path.GetDirectoryName(assets.Value[i].Path);

                    if (sourceDirectory == null)
                        continue;

                    foreach (var file in Directory.GetFiles(sourceDirectory))
                        File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)));

                    bar.TickBar();
                }

        logger.LogInformation("Written all assets to directory: {Path}", rConfig.RemovedDuplicateDirectory);
    }

    public static bool AreFileContentsEqual(string path1, string path2) =>
        File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
}
