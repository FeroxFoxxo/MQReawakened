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
    public void Initialize() => sink.WorldLoad += Load;

    public void Load()
    {
        console.AddCommand(
            "clearWebCache",
            "Clears the Web Player cache manually.",
            NetworkType.Client,
            _ =>
            {
                EmptyWebCacheDirectory();
                game.AskIfRestart();
            }
        );

        RemoveWebCacheOnStart();
    }

    public bool EmptyWebCacheDirectory()
    {
        replaceCaches.CurrentlyLoadedAssets.Clear();

        rwConfig.GetWebPlayerInfoFile(rConfig, logger);

        if (string.IsNullOrEmpty(rwConfig.WebPlayerInfoFile))
            return false;

        InternalDirectory.Empty(Path.GetDirectoryName(rwConfig.WebPlayerInfoFile));
        return true;
    }

    public void RemoveWebCacheOnStart()
    {
        if (!rwConfig.FlushCacheOnStart)
            return;

        InternalDirectory.Empty(rConfig.BundleSaveDirectory);

        var shouldDelete = logger.Ask(
            "You have 'FLUSH CACHE ON START' enabled, which may delete cached files from the original game, as they use the same directory. " +
            "Please ensure, if this is your first time running this project, that there are not files already in this directory. " +
            "These would otherwise be valuable.\n" +
            $"Please note: The WEB PLAYER cache is found in your {rConfig.DefaultWebPlayerCacheLocation} folder. " +
            "Please make an __info file in here if it does not exist already.", false
        );

        if (!shouldDelete)
            return;

        var hasDeleted = !EmptyWebCacheDirectory();

        logger.LogError("Empty web caches has been run. Deleted: {HasDeleted}", hasDeleted);
    }
}
