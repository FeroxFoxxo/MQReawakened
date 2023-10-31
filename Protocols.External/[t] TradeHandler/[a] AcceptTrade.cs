using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Temporary;

namespace Protocols.External._t__TradeHandler;

public class AcceptTrade : ExternalProtocol
{
    public override string ProtocolName => "ta";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var tradedPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == message[5]);

        if (tradedPlayer == null)
            return;

        Player.TempData.TradeModel = new TradeModel(tradedPlayer);
        tradedPlayer.TempData.TradeModel = new TradeModel(Player);

        Player.SendXt("ta",
            tradedPlayer.Character.Data.CharacterName,
            tradedPlayer.Character.Data.GetLightCharacterData()
        );

        tradedPlayer.SendXt("ta",
            Player.Character.Data.CharacterName,
            Player.Character.Data.GetLightCharacterData()
        );
    }
}

