using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.BundleHost.Events;
using Server.Reawakened.BundleHost.Events.Arguments;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.Core.Configs;

namespace Server.Reawakened.BundleHost.Services;

public class BuildLevelFiles(AssetEventSink eventSink, ILogger<BuildXmlFiles> logger, ServerRConfig sConfig) : IService
{
    public readonly Dictionary<string, string> LevelFiles = [];

    public void Initialize() => eventSink.AssetBundlesLoaded += LoadLevelFiles;

    private void LoadLevelFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        logger.LogDebug("Reading level files from bundles");

        LevelFiles.Clear();

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type is AssetInfo.TypeAsset.Level)
            .ToArray();

        InternalDirectory.OverwriteDirectory(sConfig.LevelSaveDirectory);

        foreach (var asset in assets)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(asset.CacheTime);

            var text = asset.GetXmlData();

            if (string.IsNullOrEmpty(text))
            {
                logger.LogTrace("XML for {assetName} is empty! Skipping...", asset.Name);
                continue;
            }

            var path = Path.Join(sConfig.LevelSaveDirectory, $"{asset.Name}.xml");

            File.WriteAllText(path, text);

            LevelFiles.Add(asset.Name, path);
        }
    }
}
