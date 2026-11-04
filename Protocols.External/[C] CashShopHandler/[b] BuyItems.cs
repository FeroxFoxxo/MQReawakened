using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;

namespace Protocols.External._C__CashShopHandler;

public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "Cb";

    public InternalAchievement InternalAchievement { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BuyItems> Logger { get; set; }
    public VendorCatalog VendorCatalog { get; set; }

    public override void Run(string[] message)
    {
        var cashShop = (Cashshop)int.Parse(message[5]);

        if (cashShop != Cashshop.CashShop)
        {
            Logger.LogWarning("Unknown cashshop of type {Type}!", cashShop);
            return;
        }

        var items = message[6].Split('|');

        var itemList = new Dictionary<ItemDescription, int>();

        var price = 0;

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) 
                continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);

            var timeNow = DateTime.UtcNow;

            var addPrice = itemDescription.RegularPrice;
            if (itemDescription.isDiscounted())
                addPrice = itemDescription.DiscountPrice;

            price += addPrice * amount;
            itemList.Add(itemDescription, amount);
        }

        if (price > Player.Character.NCash && price > 1)
        {
            Player.SendXt("Ce", -1);
            return;
        }

        var valid = false;

        var catalogs = VendorCatalog.GetCurrentCashShop().VendorCatalogIds;

        foreach (var item in itemList)
            foreach (var catalog in catalogs)
                if (VendorCatalog.GetVendorItemList(catalog).Contains(item.Key.ItemId))
                {
                    valid = true;
                    break;
                }

        if (!valid)
        {
            Player.SendXt("wm", "302");
            return;
        }

        foreach (var itemDescription in itemList)
        {
            price = itemDescription.Key.RegularPrice;
            if (itemDescription.Key.isDiscounted())
                price = itemDescription.Key.DiscountPrice;

            Player.RemoveNCash(price * itemDescription.Value);
            Player.AddItem(itemDescription.Key, itemDescription.Value, ItemCatalog);

            Player.CheckAchievement(AchConditionType.BuyItem, [itemDescription.Key.PrefabName], InternalAchievement, Logger);
            
            if (itemDescription.Key.IsPet())
                Player.CheckAchievement(AchConditionType.BuyPet, [itemDescription.Key.PrefabName], InternalAchievement, Logger);
        }

        Player.SendCashUpdate();
        Player.SendUpdatedInventory();
        Player.SendXt("Cb", 1);
    }
}
