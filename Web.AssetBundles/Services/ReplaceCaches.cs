using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using System.Text.Json;
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
    private readonly ILogger<ReplaceCaches> _logger;
    private readonly EventSink _sink;

    public ReplaceCaches(ServerConsole console, EventSink sink, BuildAssetList buildAssetList, AssetBundleConfig config,
        ILogger<ReplaceCaches> logger, StartGame game, IHostApplicationLifetime appLifetime)
    {
        _console = console;
        _sink = sink;
        _buildAssetList = buildAssetList;
        _config = config;
        _logger = logger;
        _game = game;
        _appLifetime = appLifetime;
    }

    public void Initialize()
    {
        _sink.WorldLoad += Load;

        _appLifetime.ApplicationStarted.Register(LogInformation);
    }

    private void LogInformation()
    {
        if (_config.UseCacheReplacementScheme)
            _logger.LogError("Note: Dynamically loading cache files is not currently supported. " +
                             "When the client requests these, the first attempt will be in light blue (INFORMATION). " +
                             "When this changes to a purple (TRACE), with no more blue queries, " +
                             "please close the client and run the 'replaceCaches' command!");
    }

    public void Load() =>
        _console.AddCommand(new ConsoleCommand("replaceCaches",
            "Replaces all generated Web Player cache files with their real counterparts.",
            _ => ReplaceWebPlayerCache()));

    private void ReplaceWebPlayerCache()
    {
        _buildAssetList.CurrentlyLoadedAssets.Clear();

        _config.GetWebPlayerInfoFile(_logger);

        if (_config.FlushCacheOnStart)
            if (_logger.Ask("Flushing the cache on start is enabled, would you like to disable this?", true))
                _config.FlushCacheOnStart = false;

        var cacheModel = new CacheModel(_buildAssetList, _config);

        _logger.LogInformation(
            "Loaded {NumAssetDict} Assets With {Caches} Caches ({TotalFiles} Total Files, {Unknown} Unidentified).",
            cacheModel.TotalAssetDictionaryFiles, cacheModel.TotalFoundCaches, cacheModel.TotalCachedAssetFiles,
            cacheModel.TotalUnknownCaches
        );

        using (var bar = new DefaultProgressBar(cacheModel.TotalFoundCaches, "Replacing Caches", _logger, _config))
        {
            foreach (var cache in cacheModel.FoundCaches)
            {
                var asset = cacheModel.GetAssetInfoFromCacheName(cache.Key);

                bar.SetMessage($"Overwriting {cache.Key} ({asset.Name})");

                foreach (var cachePath in cache.Value)
                    File.Copy(asset.Path, cachePath, true);

                bar.TickBar();
            }
        }

        Directory.CreateDirectory(_config.AssetSaveDirectory);
        var replacementLogPath = Path.Join(_config.AssetSaveDirectory, "replacedAssets.json");
        File.WriteAllText(
            replacementLogPath,
            JsonSerializer.Serialize(cacheModel, new JsonSerializerOptions { WriteIndented = true })
        );

        _logger.LogInformation("Logged Cache Replacements To {ReplacementFile}", replacementLogPath);

        _game.AskIfRestart();
    }
}
