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
        var traderModel = Player.TempData.TradeModel;

        if (traderModel == null)
            return;

        var traderName = message[5];
        var tradedPlayer = PlayerHandler.GetPlayerByName(traderName);

        if (tradedPlayer == null)
            return;

        if (tradedPlayer != traderModel.TradingPlayer)
            return;

        var tradeeModel = tradedPlayer.TempData.TradeModel;

        if (tradeeModel == null)
            return;

        tradeeModel.AcceptedTrade = true;
        traderModel.AcceptedTrade = true;

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

