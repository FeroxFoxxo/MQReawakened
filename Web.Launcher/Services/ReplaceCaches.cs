using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.BundleHost.Services;

namespace Web.Launcher.Services;

public class ReplaceCaches(ServerConsole console, EventSink sink, BuildAssetList buildAssetList,
    AssetBundleRConfig rConfig, ILogger<ReplaceCaches> logger, StartGame game,
    AssetBundleRwConfig rwConfig) : IService
{
    private readonly object _lock = new();

    public readonly List<string> CurrentlyLoadedAssets = [];
    public readonly List<string> ReplacedBundles = [];

    public void Initialize() => sink.WorldLoad += Load;

    public void Load() =>
        console.AddCommand(
            "replaceCaches",
            "Replaces all generated Web Player cache files with their real counterparts.",
            NetworkType.Client,
            _ => ReplaceWebPlayerCache(false, true)
        );

    public void ReplaceWebPlayerCache(bool checkHasCached, bool startAfterReplace)
    {
        if (checkHasCached && CurrentlyLoadedAssets.Count == 0)
            return;

        CurrentlyLoadedAssets.Clear();

        rwConfig.GetWebPlayerInfoFile(rConfig, logger);

        if (string.IsNullOrEmpty(rwConfig.WebPlayerInfoFile))
            return;

        lock (_lock)
            if (rwConfig.FlushCacheOnStart)
                if (logger.Ask("Flushing the cache on start is enabled, would you like to disable this?", true))
                    rwConfig.FlushCacheOnStart = false;

        var cacheModel = new CacheModel(buildAssetList, rwConfig);

        logger.LogInformation(
            "Loaded {NumAssetDict} assets with {Caches} caches ({TotalFiles} total files, {Unknown} unidentified).",
            cacheModel.TotalAssetDictionaryFiles, cacheModel.TotalFoundCaches, cacheModel.TotalCachedAssetFiles,
            cacheModel.TotalUnknownCaches
        );

        using (var bar = new DefaultProgressBar(cacheModel.TotalFoundCaches, "Replacing Caches", logger, rwConfig))
            foreach (var cache in cacheModel.FoundCaches)
            {
                var asset = cacheModel.GetAssetInfoFromCacheName(cache.Key);

                lock (_lock)
                    foreach (var cachePath in cache.Value.Where(cachePath => !ReplacedBundles.Contains(cachePath))
                                 .Where(File.Exists))
                    {
                        File.Copy(asset.Path, cachePath, true);
                        ReplacedBundles.Add(cachePath);

                        bar.SetMessage($"Overwriting {cache.Key} ({asset.Name})");
                    }

                bar.TickBar();
            }

        if (startAfterReplace)
            game.AskIfRestart();
    }
}
