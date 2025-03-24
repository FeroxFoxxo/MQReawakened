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

    public override void Run(string[] message)
    {
        var items = message[6].Split('|');

        // On 2014, vendorGoId[5] is the vendor id (unused)
        var vendorGoId = int.Parse(message[ServerConfig.GameVersion >= GameVersion.vEarly2014 ? 7 : 5]);

        var bought = new List<Tuple<ItemDescription, int>>();
        var bananaPrice = 0;
        var cashPrice = 0;

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) 
                continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);

            if (itemDescription.IsBananaItem())
                bananaPrice += itemDescription.RegularPrice * amount;
            else if (itemDescription.IsNcItem())
                cashPrice += itemDescription.RegularPrice * amount;

            bought.Add(new(itemDescription, amount));
        }

        if (bananaPrice > Player.Character.Cash && bananaPrice > 1
            || cashPrice > Player.Character.NCash && cashPrice > 1)
        {
            Player.SendXt("Ce", -1);
            return;
        }

        foreach (var item in bought)
        {
            if (item.Item1.IsBananaItem())
                Player.RemoveBananas(item.Item1.RegularPrice * item.Item2);
            else if (item.Item1.IsNcItem())
                Player.RemoveNCash(item.Item1.RegularPrice * item.Item2);

            Player.AddItem(item.Item1, item.Item2, ItemCatalog);

            Player.CheckObjective(ObjectiveEnum.Buyitem, vendorGoId.ToString(), item.Item1.PrefabName, item.Item2, ItemCatalog);

            Player.CheckAchievement(AchConditionType.BuyItem, [item.Item1.PrefabName], InternalAchievement, Logger);
            Player.CheckAchievement(AchConditionType.BuyPet, [item.Item1.PrefabName], InternalAchievement, Logger);
        }

        Player.SendUpdatedInventory();
    }
}
