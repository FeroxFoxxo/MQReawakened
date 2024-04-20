using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Extensions;

namespace Web.Launcher.Services;

public class ClearWebCaches(ILogger<ClearWebCaches> logger, AssetBundleRConfig rConfig,
    ServerConsole console, EventSink sink, StartGame game,
    AssetBundleRwConfig rwConfig, ReplaceCaches replaceCaches) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load() =>
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

    public bool EmptyWebCacheDirectory()
    {
        replaceCaches.CurrentlyLoadedAssets.Clear();

        rwConfig.GetWebPlayerInfoFile(rConfig, logger);

        if (string.IsNullOrEmpty(rwConfig.WebPlayerInfoFile))
            return false;

        InternalDirectory.Empty(Path.GetDirectoryName(rwConfig.WebPlayerInfoFile));
        return true;
    }
}
