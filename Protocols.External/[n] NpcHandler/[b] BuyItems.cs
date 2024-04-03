using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._n__NpcHandler;

public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "nb";

    public ServerRConfig ServerConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<BuyItems> Logger { get; set; }

    public override void Run(string[] message)
    {
        var items = message[6].Split('|');

        // On 2014, vendorGoId[5] is the vendor id (unused)
        var vendorGoId = int.Parse(message[ServerConfig.GameVersion >= GameVersion.v2014 ? 7 : 5]);

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);

            Player.AddItem(itemDescription, amount, ItemCatalog);

            if (itemDescription.Currency == CurrencyType.Banana)
                Player.RemoveBananas(itemDescription.RegularPrice * amount);
            else if (itemDescription.Currency == CurrencyType.NickCash)
                Player.RemoveNCash(itemDescription.RegularPrice * amount);

            Player.CheckObjective(ObjectiveEnum.Buyitem, vendorGoId.ToString(), itemDescription.PrefabName, amount, ItemCatalog);

            Player.CheckAchievement(AchConditionType.BuyItem, itemDescription.PrefabName, InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.BuyPet, itemDescription.PrefabName, InternalAchievement, Logger);
        }

        Player.SendUpdatedInventory();
    }
}
