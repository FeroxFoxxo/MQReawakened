using Server.Reawakened.Core.Models;
using Server.Reawakened.Core.Network.Protocols;

namespace Protocols.External._D__DebugHandler;

public class DebugValues : ExternalProtocol
{
    public override string ProtocolName => "Dg";

    public ServerConfig Config { get; set; }

    public override void Run(string[] message) =>
        SendXt("Dg", string.Join('|', 
            Config.DefaultDebugVariables
                .Select(x => $"{(int)x.Key}|{(x.Value ? "On" : "Off")}"))
        );
}
