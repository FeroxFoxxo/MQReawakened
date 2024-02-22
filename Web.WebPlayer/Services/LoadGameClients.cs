using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Web.WebPlayer.Configs;

namespace Web.WebPlayer.Services;

public class LoadGameClients(EventSink eventSink, WebPlayerRConfig rConfig, WebPlayerRwConfig rwConfig) : IService
{
    public Dictionary<string, string> ClientFiles = [];

    public void Initialize() => eventSink.ServerStarted += LoadClients;

    private void LoadClients(ServerStartedEventArgs _)
    {
        ClientFiles = Directory.GetFiles(rConfig.GameFolder, "*.unity3d", SearchOption.AllDirectories)
            .ToDictionary(Path.GetFileNameWithoutExtension, x => x);

        if (string.IsNullOrEmpty(rwConfig.DefaultWebPlayer))
            rwConfig.DefaultWebPlayer = ClientFiles.FirstOrDefault().Key;
    }
}
