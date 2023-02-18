using Microsoft.Extensions.Logging;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Web.AssetBundles.Models;
using Web.Launcher.Models;

namespace Web.AssetBundles.Services;

public class RemoveDuplicates
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
        _console.AddCommand(new ConsoleCommand("removeDuplicates",
            "Creates a directory that does not include duplicated caches.",
            _ => RemoveDuplicateFiles()));

    private void RemoveDuplicateFiles()
    {
        _logger.LogInformation("Removing Duplicates");

        var assetList = new Dictionary<string, List<InternalAssetInfo>>();
        var assetDict = File.ReadAllText(_buildAssetList.AssetDictLocation);
        var allAssets = BuildAssetList.GetAssetsFromDictionary(assetDict).ToList();

        foreach (var asset in allAssets)
        {
            if (!assetList.ContainsKey(asset.Name))
                assetList.Add(asset.Name, new List<InternalAssetInfo>());

            var hasFoundExisting = false;

            foreach (var containedAsset in assetList[asset.Name]
                         .Where(containedAsset => containedAsset.BundleSize == asset.BundleSize &&
                                                  containedAsset.UnityVersion == asset.UnityVersion &&
                                                  containedAsset.Locale == asset.Locale &&
                                                  containedAsset.Type == asset.Type &&
                                                  Path.GetFileName(containedAsset.Path) == Path.GetFileName(asset.Path)
                         )
                    )
            {
                if (containedAsset.CacheTime > asset.CacheTime && _startConfig.Is2014Client ||
                    containedAsset.CacheTime < asset.CacheTime && !_startConfig.Is2014Client)
                    assetList[asset.Name].Remove(containedAsset);
                else
                    hasFoundExisting = true;
                break;
            }

            if (!hasFoundExisting)
                assetList[asset.Name].Add(asset);
        }

        var replacedCount = assetList.Sum(s => s.Value.Count);

        _logger.LogDebug("Removed {Count} duplicates, asset count: {Total} (of {OldTotal})",
            allAssets.Count - replacedCount, replacedCount, allAssets.Count);

        _logger.LogInformation("Writing Assets");

        Directory.CreateDirectory(_sConfig.RemovedDuplicateDirectory);

        _logger.LogDebug("Emptying duplicated directory folder...");
        GetDirectory.Empty(_sConfig.RemovedDuplicateDirectory);
        _logger.LogDebug("Emptied folder");

        var directories = new List<string>();
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
            for (var i = 0; i < totalDirectories; i++)
            {
                var path = Path.Combine(_sConfig.RemovedDuplicateDirectory, $"Cache_{i}");
                directories.Add(path);
                Directory.CreateDirectory(path);

                bar.TickBar();
            }

            foreach (var assets in assetList)
            {
                for (var i = 0; i < assets.Value.Count; i++)
                {
                    var assetPath = Path.Combine(directories[i], assets.Key);
                    Directory.CreateDirectory(assetPath);

                    var directory = Path.GetDirectoryName(assets.Value[i].Path);

                    if (directory == null)
                        continue;

                    var files = Directory.GetFiles(directory);

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        var newFile = Path.Combine(assetPath, fileName);
                        File.Copy(file, newFile);
                    }

                    bar.TickBar();
                }
            }
        }

        _logger.LogDebug("Written all assets to directory: {Path}", _sConfig.RemovedDuplicateDirectory);
    }
}
