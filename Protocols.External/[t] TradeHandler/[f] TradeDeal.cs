using A2m.Server;
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
        var traderId = Player.Character.Data.TraderId;

        var originPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 1);
        var otherPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 2);

        var originItemsInTrade = originPlayer.Character.ItemsInTrade;
        var otherItemsInTrade = otherPlayer.Character.ItemsInTrade;

        if (traderId == 1)
        {
            originPlayer.Character.Data.TradeDeal = true;

            foreach (var item in otherItemsInTrade)
            {
                var itemDescription = ItemCatalog.GetItemFromId(item.Key);
                originPlayer.Character.AddItem(itemDescription, item.Value);
                otherPlayer.Character.RemoveItem(itemDescription, item.Value);
                originPlayer.AddBananas(otherPlayer.Character.Data.BananasInTrade);
            }
            CompleteTrade(originPlayer, otherPlayer);
            originPlayer.SendXt("tf", otherPlayer.Character.Data.CharacterName);
        }

        if (traderId == 2)
        {
            otherPlayer.Character.Data.TradeDeal = true;

            foreach (var item in originItemsInTrade)
            {
                var itemDescription = ItemCatalog.GetItemFromId(item.Key);
                otherPlayer.Character.AddItem(itemDescription, item.Value);
                originPlayer.Character.RemoveItem(itemDescription, item.Value);
                otherPlayer.AddBananas(originPlayer.Character.Data.BananasInTrade);
            }
            CompleteTrade(otherPlayer, originPlayer);
            otherPlayer.SendXt("tf", originPlayer.Character.Data.CharacterName);
        }                  
    }

    public static void CompleteTrade(Player firstPlayer, Player secondPlayer)
    {
        if (firstPlayer.Character.Data.TradeDeal == true && secondPlayer.Character.Data.TradeDeal == true)
        {
            firstPlayer.SendXt("tt", string.Empty);
            secondPlayer.SendXt("tt", string.Empty);

            firstPlayer.Character.Data.BananasInTrade = 0;
            secondPlayer.Character.Data.BananasInTrade = 0;

            firstPlayer.SendUpdatedInventory(false);
            secondPlayer.SendUpdatedInventory(false);
        }
    }
}
