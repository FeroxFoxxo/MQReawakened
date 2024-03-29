using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._n__NpcHandler;

public class SellItems : ExternalProtocol
{
    public override string ProtocolName => "ns";

    public ILogger<SellItems> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

    public override void Run(string[] message)
    {
        var items = message[6].Split('|');

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item))
                continue;

            var args = item.Split(":");
            var itemId = int.Parse(args[0]);
            var amount = int.Parse(args[1]);

            var itemDescription = ItemCatalog.GetItemFromId(itemId);

            Player.RemoveItem(itemDescription, amount, ItemCatalog);

            Player.AddBananas(itemDescription.SellPrice * amount, InternalAchievement, Logger);

            Player.SendUpdatedInventory();
        }
    }
}
