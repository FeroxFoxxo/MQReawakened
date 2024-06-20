using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._t__TradeHandler;

public class TradeDeal : ExternalProtocol
{
    public override string ProtocolName => "tf";

    public ItemCatalog ItemCatalog { get; set; }
    public ItemRConfig ItemRConfig { get; set; }

    public override void Run(string[] message)
    {
        var originTradeModel = Player.TempData.TradeModel;
        if (originTradeModel == null)
            return;

        var tradingPlayer = originTradeModel.TradingPlayer;
        if (tradingPlayer == null)
            return;

        var otherTradeModel = tradingPlayer.TempData.TradeModel;
        if (otherTradeModel == null)
            return;

        if (originTradeModel.FinalisedTrade)
        {
            otherTradeModel.FinalisedTrade = true;
            Player.SendXt("tf", tradingPlayer.CharacterName);
        }

        else if (!originTradeModel.FinalisedTrade)
        {
            originTradeModel.FinalisedTrade = true;
            tradingPlayer.SendXt("tf", Player.CharacterName);
        }

        if (originTradeModel.FinalisedTrade && otherTradeModel.FinalisedTrade)
        {
            tradingPlayer.TradeWithPlayer(ItemCatalog, ItemRConfig);
            Player.TradeWithPlayer(ItemCatalog, ItemRConfig);

            tradingPlayer.SendCashUpdate();
            tradingPlayer.SendUpdatedInventory();

            Player.SendCashUpdate();
            Player.SendUpdatedInventory();

            tradingPlayer.SendXt("tt", string.Empty);
            Player.SendXt("tt", string.Empty);

            Player.RemoveTrade();
        }
    }
}
