using Server.Reawakened.Core.Network.Protocols;

namespace Protocols.External._P__PingHandler;

public class Ping : ExternalProtocol
{
    public override string ProtocolName => "Pp";

    public override void Run(string[] message) => SendXt("Pp", message[5]);
}
