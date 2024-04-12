using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._j__LootHandler;
public class OnPlayerWantToRoll : ExternalProtocol
{
    public override string ProtocolName => "jr";

    public override void Run(string[] message)
    {
        var objectId = int.Parse(message[5]);

        Player.VoteForItem(objectId, true);
    }
}
