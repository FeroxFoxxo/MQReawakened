using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

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
