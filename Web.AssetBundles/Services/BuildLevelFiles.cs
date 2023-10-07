using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Configs;
using Web.AssetBundles.Events;
using Web.AssetBundles.Events.Arguments;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildLevelFiles(AssetEventSink eventSink, ILogger<BuildXmlFiles> logger, ServerRConfig sConfig,
    AssetBundleRwConfig aBConfig) : IService
{
    private readonly AssetBundleRwConfig _aBConfig = aBConfig;
    private readonly AssetEventSink _eventSink = eventSink;
    private readonly ILogger<BuildXmlFiles> _logger = logger;
    private readonly ServerRConfig _sConfig = sConfig;

    public readonly Dictionary<string, string> LevelFiles = new();

    public void Initialize() => _eventSink.AssetBundlesLoaded += LoadLevelFiles;

    private void LoadLevelFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        _logger.LogDebug("Reading level files from bundles");

        LevelFiles.Clear();

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type is AssetInfo.TypeAsset.Level)
            .ToArray();

        InternalDirectory.OverwriteDirectory(_sConfig.LevelSaveDirectory);

        using var bar = new DefaultProgressBar(assets.Length, "Loading Level Files", _logger, _aBConfig);

        foreach (var asset in assets)
        {
            var text = asset.GetXmlData(bar);

            if (string.IsNullOrEmpty(text))
            {
                bar.SetMessage($"XML for {asset.Name} is empty! Skipping...");
                continue;
            }

            var path = Path.Join(_sConfig.LevelSaveDirectory, $"{asset.Name}.xml");

            bar.SetMessage($"Writing file to {path}");

            File.WriteAllText(path, text);

            LevelFiles.Add(asset.Name, path);

            bar.TickBar();
        }
    }
}
