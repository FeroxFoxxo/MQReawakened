using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class InviteToTrade : ExternalProtocol
{
    public override string ProtocolName => "ti";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var traderName = message[5];
        var invitedPlayer = PlayerHandler.GetPlayerByName(traderName);

        invitedPlayer?.SendXt("ti", Player.CharacterName);
    }
}
