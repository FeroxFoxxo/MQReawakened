using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class PropositionRejected : ExternalProtocol
{
    public override string ProtocolName => "te";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var playerTradeModel = Player.TempData.TradeModel;

        if (playerTradeModel == null)
            return;

        var tradingPlayer = playerTradeModel.TradingPlayer;

        if (tradingPlayer == null)
            return;

        var tradeeTradeModel = tradingPlayer.TempData.TradeModel;

        if (tradeeTradeModel == null)
            return;

        playerTradeModel.ResetTrade();
        tradeeTradeModel.ResetTrade();

        tradingPlayer.SendXt("te", Player.CharacterName);
    }
}
