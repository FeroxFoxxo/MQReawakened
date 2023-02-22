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

public class ClearWebCaches : IService
{
    private readonly AssetBundleConfig _config;
    private readonly ServerConsole _console;
    private readonly StartGame _game;
    private readonly ILogger<ClearWebCaches> _logger;
    private readonly ReplaceCaches _replaceCaches;
    private readonly AssetBundleStaticConfig _sConfig;
    private readonly EventSink _sink;

    public ClearWebCaches(ILogger<ClearWebCaches> logger, AssetBundleStaticConfig sConfig,
        ServerConsole console, EventSink sink, StartGame game,
        AssetBundleConfig config, ReplaceCaches replaceCaches)
    {
        _logger = logger;
        _sConfig = sConfig;
        _console = console;
        _sink = sink;
        _game = game;
        _config = config;
        _replaceCaches = replaceCaches;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(
            "clearWebCache",
            "Clears the Web Player cache manually.",
            NetworkType.Client,
            _ =>
            {
                EmptyWebCacheDirectory();
                _game.AskIfRestart();
            }
        );

        RemoveWebCacheOnStart();
    }

    public bool EmptyWebCacheDirectory()
    {
        _replaceCaches.CurrentlyLoadedAssets.Clear();

        _config.GetWebPlayerInfoFile(_sConfig, _logger);

        if (string.IsNullOrEmpty(_config.WebPlayerInfoFile))
            return false;

        GetDirectory.Empty(Path.GetDirectoryName(_config.WebPlayerInfoFile));
        return true;
    }

    public void RemoveWebCacheOnStart()
    {
        if (!_config.FlushCacheOnStart)
            return;

        GetDirectory.Empty(_sConfig.BundleSaveDirectory);
        
        var shouldDelete = _logger.Ask(
                "You have 'FLUSH CACHE ON START' enabled, which may delete cached files from the original game, as they use the same directory. " +
                "Please ensure, if this is your first time running this project, that there are not files already in this directory. " +
                "These would otherwise be valuable.\n" +
                $"Please note: The WEB PLAYER cache is found in your {_sConfig.DefaultWebPlayerCacheLocation} folder. " +
                "Please make an __info file in here if it does not exist already.", false
            );

        if (!shouldDelete)
            return;

        var hasDeleted = !EmptyWebCacheDirectory();

        _logger.LogError("Empty web caches has been run. Deleted: {HasDeleted}", hasDeleted);
    }
}
