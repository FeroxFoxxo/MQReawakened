using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._M__MinigameHandler;
public class WaypointReached : ExternalProtocol
{
    public override string ProtocolName => "Mw";

    public override void Run(string[] message)
    {
        var minigameObjectId = message[5];
        var waypointId = message[6];

        Player.SendXt("My", minigameObjectId, waypointId, Player.GameObjectId);
    }
}
