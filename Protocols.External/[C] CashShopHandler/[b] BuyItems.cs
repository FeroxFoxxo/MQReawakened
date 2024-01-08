using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._C__CashShopHandler;

public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "Cb";

    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BuyItems> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var cashShop = (Cashshop)int.Parse(message[5]);

        if (cashShop != Cashshop.CashShop)
        {
            Logger.LogWarning("Unknown cashshop of type {Type}!", cashShop);
            return;
        }

        var items = message[6].Split('|');

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);
            Player.RemoveNCash(itemDescription.RegularPrice * amount);

            character.AddItem(ItemCatalog.GetItemFromId(itemId), amount);
        }
        Player.SendCashUpdate();
        Player.SendUpdatedInventory(false);
    }
}
