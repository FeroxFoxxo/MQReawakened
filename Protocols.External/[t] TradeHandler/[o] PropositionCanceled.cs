using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._t__TradeHandler;

public class PropositionCanceled : ExternalProtocol
{
    public override string ProtocolName => "to";

    public override void Run(string[] message)
    {
        var playerTradeModel = Player.TempData.TradeModel;

        if (playerTradeModel == null)
            return;

        var tradingPlayer = playerTradeModel.TradingPlayer;

        if (tradingPlayer == null)
            return;

        playerTradeModel.ResetTrade();

        tradingPlayer?.SendXt("to", Player.CharacterName);
    }
}
