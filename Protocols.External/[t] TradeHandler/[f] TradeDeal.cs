using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._t__TradeHandler;

public class TradeDeal : ExternalProtocol
{
    public override string ProtocolName => "tf";

    public PlayerHandler PlayerHandler { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var tradeModel = Player.TempData.TradeModel;

        if (tradeModel == null)
            return;

        var tradingPlayer = tradeModel.TradingPlayer;

        if (tradingPlayer == null)
            return;

        if (!tradeModel.FinalisedTrade)
        {
            tradeModel.FinalisedTrade = true;
            Player.SendXt("tf", tradingPlayer.CharacterName);
        }

        if (tradingPlayer.TempData.TradeModel.FinalisedTrade)
        {
            tradingPlayer.TradeWithPlayer(ItemCatalog);
            Player.TradeWithPlayer(ItemCatalog);

            tradingPlayer.SendCashUpdate();
            tradingPlayer.SendUpdatedInventory(false);

            Player.SendCashUpdate();
            Player.SendUpdatedInventory(false);

            tradingPlayer.SendXt("tt", string.Empty);
            Player.SendXt("tt", string.Empty);

            Player.RemoveTrade();
        }
    }
}
