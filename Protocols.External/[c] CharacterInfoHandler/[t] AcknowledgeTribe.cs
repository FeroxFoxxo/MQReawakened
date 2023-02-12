using A2m.Server;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class AcknowledgeTribe : ExternalProtocol
{
    public override string ProtocolName => "ct";

    public override void Run(string[] message)
    {
        var tribe = (TribeType)int.Parse(message[5]);
        NetState.DiscoverTribe(tribe);
    }
}
