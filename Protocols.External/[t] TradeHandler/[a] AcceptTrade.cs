using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class AcceptTrade : ExternalProtocol
{
    public override string ProtocolName => "ta";

    public DatabaseContainer DatabaseContainer { get; set; }

    public override void Run(string[] message)
    {
        var originTradeModel = Player.TempData.TradeModel;

        if (originTradeModel == null)
            return;

        var traderName = message[5];
        var tradedPlayer = DatabaseContainer.GetPlayerByName(traderName);

        if (tradedPlayer == null)
            return;

        if (tradedPlayer != originTradeModel.TradingPlayer)
            return;

        var otherTradeModel = tradedPlayer.TempData.TradeModel;

        if (otherTradeModel == null)
            return;

        otherTradeModel.AcceptedTrade = true;
        originTradeModel.AcceptedTrade = true;

        Player.SendXt("ta",
            tradedPlayer.CharacterName,
            tradedPlayer.Character.Data.GetLightCharacterData()
        );

        tradedPlayer.SendXt("ta",
            Player.CharacterName,
            Player.Character.Data.GetLightCharacterData()
        );
    }
}

