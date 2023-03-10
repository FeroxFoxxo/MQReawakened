using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Web.AssetBundles.Models;
using Web.Launcher.Models;

namespace Web.AssetBundles.Services;

public class RemoveDuplicates : IService
{
    private readonly BuildAssetList _buildAssetList;
    private readonly ServerConsole _console;
    private readonly LauncherRwConfig _launcherWConfig;
    private readonly ILogger<RemoveDuplicates> _logger;
    private readonly AssetBundleRConfig _rConfig;
    private readonly AssetBundleRwConfig _rwConfig;
    private readonly EventSink _sink;

    public RemoveDuplicates(ILogger<RemoveDuplicates> logger, EventSink sink,
        ServerConsole console, LauncherRwConfig launcherWConfig, BuildAssetList buildAssetList,
        AssetBundleRConfig rConfig, AssetBundleRwConfig rwConfig)
    {
        _logger = logger;
        _sink = sink;
        _console = console;
        _launcherWConfig = launcherWConfig;
        _buildAssetList = buildAssetList;
        _rConfig = rConfig;
        _rwConfig = rwConfig;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(
            "removeDuplicates",
            "Creates a directory that does not include duplicated caches.",
            NetworkType.Server | NetworkType.Client,
            _ => RemoveDuplicateFiles()
        );
        _console.AddCommand(
            "removeXmlDuplicates",
            "Creates a directory that does not include duplicated XML files (required for servers).",
            NetworkType.Server | NetworkType.Client,
            _ => RemoveDuplicateFiles(new[] { AssetInfo.TypeAsset.Level, AssetInfo.TypeAsset.XML })
        );
    }

    private void RemoveDuplicateFiles(AssetInfo.TypeAsset[] filters = null)
    {
        _logger.LogDebug("Removing duplicates");

        var assetList = new Dictionary<string, List<InternalAssetInfo>>();
        var assetDict = File.ReadAllText(_buildAssetList.AssetDictLocation);
        var allAssets = BuildAssetList.GetAssetsFromDictionary(assetDict)
            .Where(x => filters?.Any(f => x.Type == f) ?? true)
            .ToArray();

        using (
            var bar = new DefaultProgressBar(
                allAssets.Length,
                "Reading assets from disk",
                _logger,
                _rwConfig
            )
        )
        {
            foreach (var asset in allAssets)
            {
                var assetName = asset.Name.Trim().ToLower();

                if (!assetList.ContainsKey(assetName))
                    assetList.Add(assetName, new List<InternalAssetInfo>());

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

                    if (containedAsset.CacheTime > asset.CacheTime && _launcherWConfig.Is2014Client ||
                        containedAsset.CacheTime < asset.CacheTime && !_launcherWConfig.Is2014Client)

                        assetList[assetName].Remove(containedAsset);
                    else
                        hasFoundExisting = true;

                    break;
                }

                if (!hasFoundExisting)
                    assetList[assetName].Add(asset);

                bar.TickBar();
            }
        }

        var replacedCount = assetList.Sum(s => s.Value.Count);

        _logger.LogInformation("Removed {Count} duplicates, asset count: {Total} (of {OldTotal})",
            allAssets.Length - replacedCount, replacedCount, allAssets.Length);

        _logger.LogDebug("Writing assets");

        _logger.LogDebug("Emptying duplicated directory folder...");
        InternalDirectory.Empty(_rConfig.RemovedDuplicateDirectory);
        _logger.LogDebug("Emptied folder");

        var totalDirectories = assetList.Max(s => s.Value?.Count ?? 0);

        using (
            var bar = new DefaultProgressBar(
                replacedCount + totalDirectories,
                "Writing Assets To Disk",
                _logger,
                _rwConfig
            )
        )
        {
            foreach (var assets in assetList)
            {
                for (var i = 0; i < assets.Value.Count; i++)
                {
                    var targetDirectory = Path.Combine(_rConfig.RemovedDuplicateDirectory, assets.Key, i.ToString());
                    InternalDirectory.CreateDirectory(targetDirectory);

                    var sourceDirectory = Path.GetDirectoryName(assets.Value[i].Path);

                    if (sourceDirectory == null)
                        continue;

                    foreach (var file in Directory.GetFiles(sourceDirectory))
                        File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)));

                    bar.TickBar();
                }
            }
        }

        _logger.LogInformation("Written all assets to directory: {Path}", _rConfig.RemovedDuplicateDirectory);
    }

    public static bool AreFileContentsEqual(string path1, string path2) =>
        File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
}
