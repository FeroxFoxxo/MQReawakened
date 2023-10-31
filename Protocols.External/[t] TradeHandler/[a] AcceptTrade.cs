using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;
public class AcceptTrade : ExternalProtocol
{
    public override string ProtocolName => "ta";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var otherPlayer = message[5];

        var invitedPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == otherPlayer);

        Player.Character.ItemsInTrade = [];
        invitedPlayer.Character.ItemsInTrade = [];

        Player.Character.Data.AcceptedTrade = true;
        invitedPlayer.Character.Data.AcceptedTrade = true;

        Player.Character.Data.TradeDeal = false;
        invitedPlayer.Character.Data.TradeDeal = false;

        var otherCharData = invitedPlayer.Character.Data.GetLightCharacterData();
        var originCharData = Player.Character.Data.GetLightCharacterData();

        Player?.SendXt("ta", invitedPlayer.Character.Data.CharacterName, otherCharData);
        invitedPlayer?.SendXt("ta", Player.Character.Data.CharacterName, originCharData);
    }
}

