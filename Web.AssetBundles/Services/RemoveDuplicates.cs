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
    private readonly ILogger<RemoveDuplicates> _logger;
    private readonly AssetBundleStaticConfig _sConfig;
    private readonly EventSink _sink;
    private readonly StartConfig _startConfig;

    public RemoveDuplicates(ILogger<RemoveDuplicates> logger, EventSink sink,
        AssetBundleStaticConfig sConfig, ServerConsole console, StartConfig startConfig,
        BuildAssetList buildAssetList)
    {
        _logger = logger;
        _sink = sink;
        _sConfig = sConfig;
        _console = console;
        _startConfig = startConfig;
        _buildAssetList = buildAssetList;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load() =>
        _console.AddCommand(
            "removeDuplicates",
            "Creates a directory that does not include duplicated caches.",
            NetworkType.Both,
            _ => RemoveDuplicateFiles()
        );

    private void RemoveDuplicateFiles()
    {
        _logger.LogDebug("Removing duplicates");

        var assetList = new Dictionary<string, List<InternalAssetInfo>>();
        var assetDict = File.ReadAllText(_buildAssetList.AssetDictLocation);
        var allAssets = BuildAssetList.GetAssetsFromDictionary(assetDict).ToArray();

        using (
            var bar = new DefaultProgressBar(
                allAssets.Length,
                "Reading assets from disk",
                _logger,
                _sConfig
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

                    if (containedAsset.CacheTime > asset.CacheTime && _startConfig.Is2014Client ||
                        containedAsset.CacheTime < asset.CacheTime && !_startConfig.Is2014Client)

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

        Directory.CreateDirectory(_sConfig.RemovedDuplicateDirectory);

        _logger.LogDebug("Emptying duplicated directory folder...");
        GetDirectory.Empty(_sConfig.RemovedDuplicateDirectory);
        _logger.LogDebug("Emptied folder");

        var totalDirectories = assetList.Max(s => s.Value.Count);

        using (
            var bar = new DefaultProgressBar(
                replacedCount + totalDirectories,
                "Writing Assets To Disk",
                _logger,
                _sConfig
            )
        )
        {
            foreach (var assets in assetList)
            {
                for (var i = 0; i < assets.Value.Count; i++)
                {
                    var targetDirectory = Path.Combine(_sConfig.RemovedDuplicateDirectory, assets.Key, i.ToString());
                    Directory.CreateDirectory(targetDirectory);

                    var sourceDirectory = Path.GetDirectoryName(assets.Value[i].Path);

                    if (sourceDirectory == null)
                        continue;

                    foreach (var file in Directory.GetFiles(sourceDirectory))
                        File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)));

                    bar.TickBar();
                }
            }
        }

        _logger.LogInformation("Written all assets to directory: {Path}", _sConfig.RemovedDuplicateDirectory);
    }

    public static bool AreFileContentsEqual(string path1, string path2) =>
        File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
}
