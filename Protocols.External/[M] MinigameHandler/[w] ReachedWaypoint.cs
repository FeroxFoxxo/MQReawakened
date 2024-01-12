using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Minigames;

namespace Protocols.External._M__MinigameHandler;
public class WaypointReached : ExternalProtocol
{
    public override string ProtocolName => "Mw";

    public override void Run(string[] message)
    {
        var minigameObjectId = message[5];
        var waypointId = message[6];

        foreach (var player in ArenaModel.Participants)
            player.SendXt("My", minigameObjectId, waypointId, Player.CharacterId);
    }
}
