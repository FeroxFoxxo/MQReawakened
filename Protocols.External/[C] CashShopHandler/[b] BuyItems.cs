using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._C__CashShopHandler;

public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "Cb";

    public InternalAchievement InternalAchievement { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BuyItems> Logger { get; set; }

    public override void Run(string[] message)
    {
        var cashShop = (Cashshop)int.Parse(message[5]);

        if (cashShop != Cashshop.CashShop)
        {
            Logger.LogWarning("Unknown cashshop of type {Type}!", cashShop);
            return;
        }

        var items = message[6].Split('|');

        var bought = new List<Tuple<ItemDescription, int>>();
        var price = 0;

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);

            Player.CheckAchievement(AchConditionType.BuyItem, itemDescription.PrefabName, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.BuyPet, itemDescription.PrefabName, InternalAchievement, Logger);

            price += itemDescription.RegularPrice * amount;
            bought.Add(new(itemDescription, amount));
        }

        if (price > Player.Character.Data.NCash && price > 1)
        {
            Player.SendXt("Ce", -1);
            return;
        }

        foreach (var item in bought)
        {
            Player.RemoveNCash(item.Item1.RegularPrice * item.Item2);
            Player.AddItem(item.Item1, item.Item2, ItemCatalog);
        }

        Player.SendCashUpdate();
        Player.SendUpdatedInventory();
        Player.SendXt("Cb", 1);
    }
}
