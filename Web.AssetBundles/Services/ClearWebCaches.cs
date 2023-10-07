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

public class ClearWebCaches(ILogger<ClearWebCaches> logger, AssetBundleRConfig rConfig,
    ServerConsole console, EventSink sink, StartGame game,
    AssetBundleRwConfig rwConfig, ReplaceCaches replaceCaches) : IService
{
    private readonly ServerConsole _console = console;
    private readonly StartGame _game = game;
    private readonly ILogger<ClearWebCaches> _logger = logger;
    private readonly AssetBundleRConfig _rConfig = rConfig;
    private readonly ReplaceCaches _replaceCaches = replaceCaches;
    private readonly AssetBundleRwConfig _rwConfig = rwConfig;
    private readonly EventSink _sink = sink;

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

        _rwConfig.GetWebPlayerInfoFile(_rConfig, _logger);

        if (string.IsNullOrEmpty(_rwConfig.WebPlayerInfoFile))
            return false;

        InternalDirectory.Empty(Path.GetDirectoryName(_rwConfig.WebPlayerInfoFile));
        return true;
    }

    public void RemoveWebCacheOnStart()
    {
        if (!_rwConfig.FlushCacheOnStart)
            return;

        InternalDirectory.Empty(_rConfig.BundleSaveDirectory);

        var shouldDelete = _logger.Ask(
            "You have 'FLUSH CACHE ON START' enabled, which may delete cached files from the original game, as they use the same directory. " +
            "Please ensure, if this is your first time running this project, that there are not files already in this directory. " +
            "These would otherwise be valuable.\n" +
            $"Please note: The WEB PLAYER cache is found in your {_rConfig.DefaultWebPlayerCacheLocation} folder. " +
            "Please make an __info file in here if it does not exist already.", false
        );

        if (!shouldDelete)
            return;

        var hasDeleted = !EmptyWebCacheDirectory();

        _logger.LogError("Empty web caches has been run. Deleted: {HasDeleted}", hasDeleted);
    }
}
