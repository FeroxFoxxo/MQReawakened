using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._p__GroupHandler;

public class LeaveGroup : ExternalProtocol
{
    public override string ProtocolName => "pl";

    public override void Run(string[] message) => Player.RemoveFromGroup();
}
