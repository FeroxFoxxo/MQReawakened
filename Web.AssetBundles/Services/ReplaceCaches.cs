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

public class ReplaceCaches : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly BuildAssetList _buildAssetList;
    private readonly AssetBundleConfig _config;
    private readonly ServerConsole _console;
    private readonly StartGame _game;
    private readonly object _lock;
    private readonly ILogger<ReplaceCaches> _logger;
    private readonly AssetBundleStaticConfig _sConfig;
    private readonly EventSink _sink;

    public readonly List<string> CurrentlyLoadedAssets;
    public readonly List<string> ReplacedBundles;

    public ReplaceCaches(ServerConsole console, EventSink sink, BuildAssetList buildAssetList,
        AssetBundleStaticConfig sConfig,
        ILogger<ReplaceCaches> logger, StartGame game, IHostApplicationLifetime appLifetime, AssetBundleConfig config)
    {
        _console = console;
        _sink = sink;
        _buildAssetList = buildAssetList;
        _logger = logger;
        _game = game;
        _appLifetime = appLifetime;
        _sConfig = sConfig;
        _config = config;

        CurrentlyLoadedAssets = new List<string>();
        ReplacedBundles = new List<string>();
        _lock = new object();
    }

    public void Initialize()
    {
        _sink.WorldLoad += Load;

        _appLifetime.ApplicationStarted.Register(LogDynamicAssetWarning);
    }

    private void LogDynamicAssetWarning()
    {
        if (_sConfig.UseCacheReplacementScheme)
            _logger.LogError("Note: Dynamically loading cache files is not currently supported. " +
                             "When the client requests these, the first attempt will be in light blue (INFORMATION). " +
                             "When this changes to a purple (TRACE), with no more blue queries, " +
                             "please close the client and run the 'replaceCaches' command!");
    }

    public void Load() =>
        _console.AddCommand(
            "replaceCaches",
            "Replaces all generated Web Player cache files with their real counterparts.",
            NetworkType.Client,
            _ => ReplaceWebPlayerCache()
        );

    public void ReplaceWebPlayerCache()
    {
        _config.GetWebPlayerInfoFile(_sConfig, _logger);

        if (string.IsNullOrEmpty(_config.WebPlayerInfoFile))
            return;

        lock (_lock)
        {
            if (_config.FlushCacheOnStart)
                if (_logger.Ask("Flushing the cache on start is enabled, would you like to disable this?", true))
                    _config.FlushCacheOnStart = false;
        }

        var cacheModel = new CacheModel(_buildAssetList, _config);

        _logger.LogInformation(
            "Loaded {NumAssetDict} assets with {Caches} caches ({TotalFiles} total files, {Unknown} unidentified).",
            cacheModel.TotalAssetDictionaryFiles, cacheModel.TotalFoundCaches, cacheModel.TotalCachedAssetFiles,
            cacheModel.TotalUnknownCaches
        );

        CurrentlyLoadedAssets.Clear();

        using (var bar = new DefaultProgressBar(cacheModel.TotalFoundCaches, "Replacing Caches", _logger, _sConfig))
        {
            foreach (var cache in cacheModel.FoundCaches)
            {
                var asset = cacheModel.GetAssetInfoFromCacheName(cache.Key);

                ReplaceCacheFiles(asset, cache.Value);

                bar.SetMessage($"Overwriting {cache.Key} ({asset.Name})");
                bar.TickBar();
            }
        }

        _game.AskIfRestart();
    }

    private void ReplaceCacheFiles(InternalAssetInfo asset, IEnumerable<string> paths)
    {
        lock (_lock)
        {
            foreach (var cachePath in paths.Where(cachePath => !ReplacedBundles.Contains(cachePath)).Where(File.Exists))
            {
                File.Copy(asset.Path, cachePath, true);
                ReplacedBundles.Add(cachePath);
            }
        }
    }
}
