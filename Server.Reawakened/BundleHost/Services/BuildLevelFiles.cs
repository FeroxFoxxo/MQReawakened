using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Events;
using Server.Reawakened.BundleHost.Events.Arguments;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.Core.Configs;

namespace Server.Reawakened.BundleHost.Services;

public class BuildLevelFiles(AssetEventSink eventSink, ILogger<BuildXmlFiles> logger, ServerRConfig sConfig, AssetBundleRwConfig rwConfig) : IService
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

        if (rwConfig.UseCustomAssetLoader)
        {
            foreach (var asset in Directory.GetFiles(rwConfig.DatabaseLevelDirectory))
            {
                var text = string.Empty;

                if (File.Exists(asset))
                {
                    var levelXml = GetOsType.IsUnix() ? asset.Split('/')[^1] : asset.Split('\\')[^1];
                    sConfig.LoadedAssets.Add(levelXml.Split('.')[0]);
                    text = File.ReadAllText(asset);
                }

                if (text.Equals(string.Empty))
                {
                    logger.LogTrace("XML at {assetName} is empty! Skipping...", asset);
                    continue;
                }

                var file = GetOsType.IsUnix() ? asset.Split('/') : asset.Split('\\');

                var path = Path.Join(sConfig.LevelSaveDirectory, file[^1]);
                File.WriteAllText(path, text);

                LevelFiles.Add(file[^1], path);
            }
        }
        else
        {
            foreach (var asset in assets)
            {
                var time = DateTimeOffset.FromUnixTimeSeconds(asset.CacheTime);

                var text = asset.GetXmlData(rwConfig);

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
}
