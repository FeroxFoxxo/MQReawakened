using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._t__TradeHandler;

public class CancelTrade : ExternalProtocol
{
    public override string ProtocolName => "tc";

    public override void Run(string[] message)
    {
        var tradeModel = Player.TempData.TradeModel;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        Player.RemoveTrade();

        tradingPlayer?.SendXt("tc", Player.CharacterName);
    }
}
