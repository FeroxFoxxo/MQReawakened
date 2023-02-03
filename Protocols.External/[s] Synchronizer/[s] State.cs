using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._s__Synchronizer;

public class State : ExternalProtocol
{
    public SyncEventManager SyncEventManager { get; set; }
    public ILogger<State> Logger { get; set; }
    public ServerConfig ServerConfig { get; set; }

    public override string ProtocolName => "ss";

    public override void Run(string[] message)
    {
        var syncEvent = SyncEventManager.DecodeEvent(message[5].Split('&'));

        if (ServerConfig.LogSyncState)
            Logger.LogInformation("Found state: {State}", syncEvent.Type);
    }
}
