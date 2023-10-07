using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;
using Web.Launcher.Services;

namespace Web.AssetBundles.Services;

public class ReplaceCaches(ServerConsole console, EventSink sink, BuildAssetList buildAssetList,
    AssetBundleRConfig rConfig,
    ILogger<ReplaceCaches> logger, StartGame game, IHostApplicationLifetime appLifetime,
    AssetBundleRwConfig rwConfig) : IService
{
    private readonly IHostApplicationLifetime _appLifetime = appLifetime;
    private readonly BuildAssetList _buildAssetList = buildAssetList;
    private readonly ServerConsole _console = console;
    private readonly StartGame _game = game;
    private readonly object _lock = new();
    private readonly ILogger<ReplaceCaches> _logger = logger;
    private readonly AssetBundleRConfig _rConfig = rConfig;
    private readonly AssetBundleRwConfig _rwConfig = rwConfig;
    private readonly EventSink _sink = sink;

    public readonly List<string> CurrentlyLoadedAssets = new();
    public readonly List<string> ReplacedBundles = new();

    public void Initialize()
    {
        _sink.WorldLoad += Load;

        _appLifetime.ApplicationStarted.Register(EnsureCacheReplaced);
    }

    private void EnsureCacheReplaced()
    {
        if (string.IsNullOrEmpty(_rwConfig.WebPlayerInfoFile))
            return;

        ReplaceWebPlayerCache(false, false);
    }

    public void Load() =>
        _console.AddCommand(
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

        _rwConfig.GetWebPlayerInfoFile(_rConfig, _logger);

        if (string.IsNullOrEmpty(_rwConfig.WebPlayerInfoFile))
            return;

        lock (_lock)
        {
            if (_rwConfig.FlushCacheOnStart)
                if (_logger.Ask("Flushing the cache on start is enabled, would you like to disable this?", true))
                    _rwConfig.FlushCacheOnStart = false;
        }

        var cacheModel = new CacheModel(_buildAssetList, _rwConfig);

        _logger.LogInformation(
            "Loaded {NumAssetDict} assets with {Caches} caches ({TotalFiles} total files, {Unknown} unidentified).",
            cacheModel.TotalAssetDictionaryFiles, cacheModel.TotalFoundCaches, cacheModel.TotalCachedAssetFiles,
            cacheModel.TotalUnknownCaches
        );

        using (var bar = new DefaultProgressBar(cacheModel.TotalFoundCaches, "Replacing Caches", _logger, _rwConfig))
        {
            foreach (var cache in cacheModel.FoundCaches)
            {
                var asset = cacheModel.GetAssetInfoFromCacheName(cache.Key);

                lock (_lock)
                {
                    foreach (var cachePath in cache.Value.Where(cachePath => !ReplacedBundles.Contains(cachePath))
                                 .Where(File.Exists))
                    {
                        File.Copy(asset.Path, cachePath, true);
                        ReplacedBundles.Add(cachePath);

                        bar.SetMessage($"Overwriting {cache.Key} ({asset.Name})");
                    }
                }

                bar.TickBar();
            }
        }

        if (startAfterReplace)
            _game.AskIfRestart();
    }
}
