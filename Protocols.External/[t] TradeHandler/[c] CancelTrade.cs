using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class CancelTrade : ExternalProtocol
{
    public override string ProtocolName => "tc";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var tradingPlayer = Player.TempData.TradeModel?.TradingPlayer;

        tradingPlayer?.SendXt("tc", Player.CharacterName);

        Player.TempData.TradeModel = null;

        if (tradingPlayer != null)
            tradingPlayer.TempData.TradeModel = null;
    }
}
