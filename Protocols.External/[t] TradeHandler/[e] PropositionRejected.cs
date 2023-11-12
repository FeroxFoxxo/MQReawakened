using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._t__TradeHandler;

public class PropositionRejected : ExternalProtocol
{
    public override string ProtocolName => "te";

    public override void Run(string[] message)
    {
        var playerTradeModel = Player.TempData.TradeModel;

        if (playerTradeModel == null)
            return;

        var tradingPlayer = playerTradeModel.TradingPlayer;

        if (tradingPlayer == null)
            return;

        var otherTradeModel = tradingPlayer.TempData.TradeModel;

        if (otherTradeModel == null)
            return;

        otherTradeModel.ResetTrade();

        tradingPlayer?.SendXt("te", Player.CharacterName);
    }
}
