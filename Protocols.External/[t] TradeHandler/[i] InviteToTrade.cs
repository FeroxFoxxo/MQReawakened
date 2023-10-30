using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._t__TradeHandler;
public class InviteToTrade : ExternalProtocol
{
    public override string ProtocolName => "ti";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var otherPlayer = message[5];

        var invitedPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == otherPlayer);

        Player.Character.Data.TraderId = 1;
        invitedPlayer.Character.Data.TraderId = 2;

        Player.Character.Data.StoppedTrade = false;
        invitedPlayer.Character.Data.StoppedTrade = false;

        Player.Character.Data.AcceptedTrade = false;
        invitedPlayer.Character.Data.AcceptedTrade = false;

        Console.WriteLine("TraderId 1 name: " + Player.Character.Data.CharacterName);
        Console.WriteLine("TraderId 2 name: " + invitedPlayer.Character.Data.CharacterName);

        invitedPlayer?.SendXt("ti",
        Player.Character.Data.CharacterName);
    }
}
