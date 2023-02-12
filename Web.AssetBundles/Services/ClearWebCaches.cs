using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
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
    private readonly EventSink _sink;
    private readonly BuildAssetList _buildAssetList;

    public ClearWebCaches(ILogger<ClearWebCaches> logger, AssetBundleConfig config,
        ServerConsole console, EventSink sink, StartGame game, BuildAssetList buildAssetList)
    {
        _logger = logger;
        _config = config;
        _console = console;
        _sink = sink;
        _game = game;
        _buildAssetList = buildAssetList;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(new ConsoleCommand("clearWebCache", "Clears the Web Player cache manually.",
            _ =>
            {
                EmptyWebCacheDirectory();
                _game.AskIfRestart();
            }));

        RemoveWebCacheOnStart();
    }

    public bool EmptyWebCacheDirectory()
    {
        _buildAssetList.CurrentlyLoadedAssets.Clear();

        _config.GetWebPlayerInfoFile(_logger);

        if (_config.WebPlayerInfoFile == _config.CacheInfoFile)
        {
            _logger.LogError("Web player cache and saved directory should not be the same! Skipping...");
            _config.WebPlayerInfoFile = string.Empty;

            return false;
        }

        if (!_config.WebPlayerInfoFile.ToLower().Contains("appdata"))
        {
            _logger.LogError("Web player cache has to be in the AppData/LocalLow folder! Skipping...");
            _config.WebPlayerInfoFile = string.Empty;

            return false;
        }

        GetDirectory.Empty(Path.GetDirectoryName(_config.WebPlayerInfoFile));

        return true;
    }

    public void RemoveWebCacheOnStart()
    {
        if (!_config.FlushCacheOnStart)
            return;

        GetDirectory.Empty(_config.BundleSaveDirectory);

        var shouldDelete = _config.DefaultDelete;

        if (!shouldDelete)
            shouldDelete = _logger.Ask(
                "You have 'FLUSH CACHE ON START' enabled, which may delete cached files from the original game, as they use the same directory. " +
                "Please ensure, if this is your first time running this project, that there are not files already in this directory. " +
                "These would otherwise be valuable.\n" +
                $"Please note: The WEB PLAYER cache is found in your {_config.DefaultWebPlayerCacheLocation} folder. " +
                "Please make an __info file in here if it does not exist already.", false
            );

        if (!shouldDelete)
            return;

        if (_config.DefaultDelete || !EmptyWebCacheDirectory())
            return;

        if (_logger.Ask(
                "It is recommended to clean your caches each time in debug mode. " +
                "Do you want to set this as the default action?", true
            ))
            _config.DefaultDelete = true;
    }
}
