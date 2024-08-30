using Server.Base.Core.Abstractions;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Events;
using Web.Launcher.Models;

namespace Web.Launcher.Services;
public class LoadUpdates(EventSink eventSink, LauncherRConfig rConfig) : IService
{
    public Dictionary<string, string> ClientFiles = [];

    public void Initialize() => eventSink.ServerStarted += LoadClients;

    private void LoadClients(ServerStartedEventArgs _) => ClientFiles = Directory.GetFiles(rConfig.GameFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);
}
