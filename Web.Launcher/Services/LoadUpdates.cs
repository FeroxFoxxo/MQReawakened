using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Web.Launcher.Models;

namespace Web.Launcher.Services;
public class LoadUpdates(EventSink eventSink, LauncherRConfig rConfig) : IService
{
    public Dictionary<string, string> ClientFiles = [];
    public Dictionary<string, string> LauncherFiles = [];

    public void Initialize() => eventSink.ServerStarted += LoadClients;

    private void LoadClients(ServerStartedEventArgs _)
    {
        ClientFiles = Directory.GetFiles(rConfig.GameFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);

        LauncherFiles = Directory.GetFiles(rConfig.LauncherFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);
    }
}
