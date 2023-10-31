using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._p__GroupHandler;
public class PromoteMember : ExternalProtocol
{
    public override string ProtocolName => "pp";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var groupMember = message[5];

        var newLeader = PlayerHandler.PlayerList.Find(p => p.Character.Data.CharacterName == groupMember);

        SendXt("pp", newLeader.Character.Data.CharacterName);
        newLeader.SendXt("pp", newLeader.Character.Data.CharacterName);
    }
}
