using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlay : ExternalProtocol
{
    public override string ProtocolName => "ly";

    public override void Run(string[] message) => Player.SendLevelChange();
}
