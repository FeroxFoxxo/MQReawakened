using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class ClearWebCaches : IService
{
    private readonly EventSink _sink;
    private readonly ServerConsole _console;
    private readonly AssetBundleConfig _config;
    private readonly ILogger<ClearWebCaches> _logger;

    public ClearWebCaches(ILogger<ClearWebCaches> logger, AssetBundleConfig config,
        ServerConsole console, EventSink sink)
    {
        _logger = logger;
        _config = config;
        _console = console;
        _sink = sink;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(new ConsoleCommand("clearWebCache", "Clears the Web Player cache manually.",
            _ => EmptyWebCacheDirectory()));

        RemoveWebCacheOnStart();
    }

    public bool EmptyWebCacheDirectory()
    {
        _config.WebPlayerInfoFile = GetInfoFile.TryGetInfoFile($"Web Player '{_config.DefaultWebPlayerCacheLocation}'", _config.WebPlayerInfoFile, _logger);

        if (_config.WebPlayerInfoFile == _config.CacheInfoFile)
        {
            _logger.LogError("Web player cache and saved directory should not be the same! Skipping...");
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
                "Please make an __info file in here if it does not exist already."
            );

        if (!shouldDelete)
            return;
        
        if (_config.DefaultDelete || !EmptyWebCacheDirectory())
            return;

        if (_logger.Ask(
                "It is recommended to clean your caches each time in debug mode. " +
                "Do you want to set this as the default action?"
            ))
            _config.DefaultDelete = true;
    }
}
