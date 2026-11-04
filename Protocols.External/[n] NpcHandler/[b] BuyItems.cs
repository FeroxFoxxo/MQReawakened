using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;

namespace Protocols.External._n__NpcHandler;

public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "nb";

    public ServerRConfig ServerConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<BuyItems> Logger { get; set; }
    public InternalVendor InternalVendor { get; set; }
    public VendorCatalog VendorCatalog { get; set; }

    public override void Run(string[] message)
    {
        var items = message[6].Split('|');
        // On 2014, vendorGoId[5] is the vendor id (unused)
        var vendorGoId = int.Parse(message[ServerConfig.GameVersion >= GameVersion.vEarly2014 ? 7 : 5]);
        var itemList = new Dictionary<ItemDescription, int>();

        var bananaPrice = 0;
        var cashPrice = 0;

        //Hack Prevention
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);
            if (itemDescription.Currency == CurrencyType.Banana)
            {
                var price = itemDescription.RegularPrice;
                if (itemDescription.isDiscounted())
                    price = itemDescription.DiscountPrice;

                bananaPrice += price * amount;
                if (Player.Character.Cash < bananaPrice)
                {
                    Player.SendXt("Ce", -1);
                    return;
                }
            }
            else if (itemDescription.Currency == CurrencyType.NickCash)
            {
                var price = itemDescription.RegularPrice;
                if (itemDescription.isDiscounted())
                    price = itemDescription.DiscountPrice;

                cashPrice += price * amount;
                if (Player.Character.NCash < cashPrice)
                {
                    Player.SendXt("Ce", -1);
                    return;
                }
            }

            itemList.Add(itemDescription, amount);
        }

        var valid = false;

        // Check if Vendor has item
        var vendors = InternalVendor.GetVendorsById(Player.Character.LevelId, vendorGoId);

        foreach (var item in itemList)
        {
            foreach (var vendor in vendors)
            {
                if (VendorCatalog.GetVendorItemList(vendor.CatalogId).Contains(item.Key.ItemId))
                {
                    valid = true;
                    break;
                }
            }
        }

        if (!valid)
        {
            Logger.LogError("[ANTICHEAT] Player \"{Name}\" attempted to purchase invalid item from Vendor {VendorID}", Player.CharacterName, vendors[0]?.VendorId);
            Player.SendXt("wm", "302");
            return;
        }

        foreach (var itemDescription in itemList)
        {
            var price = itemDescription.Key.RegularPrice;
            if (itemDescription.Key.isDiscounted())
                price = itemDescription.Key.DiscountPrice;

            if (itemDescription.Key.Currency == CurrencyType.Banana)
                Player.RemoveBananas(price * itemDescription.Value);
            else if (itemDescription.Key.Currency == CurrencyType.NickCash)
                Player.RemoveNCash(price * itemDescription.Value);

            Player.AddItem(itemDescription.Key, itemDescription.Value, ItemCatalog);

            Player.CheckObjective(ObjectiveEnum.Buyitem, vendorGoId.ToString(), itemDescription.Key.PrefabName, itemDescription.Value, ItemCatalog);

            Player.CheckAchievement(AchConditionType.BuyItem, [itemDescription.Key.PrefabName], InternalAchievement, Logger);
            
            if (itemDescription.Key.IsPet())
                Player.CheckAchievement(AchConditionType.BuyPet, [itemDescription.Key.PrefabName], InternalAchievement, Logger);
        }

        Player.SendCashUpdate();
        Player.SendUpdatedInventory();
        Player.SendXt("nb", 1);
    }
}
