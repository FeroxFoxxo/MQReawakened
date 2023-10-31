using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class PropositionCanceled : ExternalProtocol
{
    public override string ProtocolName => "to";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var tradeModel = Player.TempData.TradeModel;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        if (tradingPlayer == null)
            return;

        tradeModel.ResetTrade();

        tradingPlayer?.SendXt("to", Player.CharacterName);
    }
}
