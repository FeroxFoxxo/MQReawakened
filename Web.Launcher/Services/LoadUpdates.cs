using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Web.Launcher.Models;

namespace Web.Launcher.Services;
public class LoadUpdates(EventSink eventSink, LauncherRConfig rConfig) : IService
{
    public Dictionary<string, string> WinClientFiles = [];
    public Dictionary<string, string> WinLauncherFiles = [];

    public Dictionary<string, string> OSXClientFiles = [];
    public Dictionary<string, string> OSXLauncherFiles = [];

    public void Initialize() => eventSink.ServerStarted += LoadClients;

    private void LoadClients(ServerStartedEventArgs _)
    {
        WinClientFiles = Directory.GetFiles(rConfig.WinGameFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);

        WinLauncherFiles = Directory.GetFiles(rConfig.WinLauncherFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);

        OSXClientFiles = Directory.GetFiles(rConfig.OSXGameFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);

        OSXLauncherFiles = Directory.GetFiles(rConfig.OSXLauncherFolder, "*.zip", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);
    }
}
