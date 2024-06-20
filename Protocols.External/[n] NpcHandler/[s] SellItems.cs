using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._n__NpcHandler;

public class SellItems : ExternalProtocol
{
    public override string ProtocolName => "ns";

    public ILogger<SellItems> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
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

            Player.RemoveItem(itemDescription, amount, ItemCatalog, ItemRConfig);

            Player.AddBananas(itemDescription.SellPrice * amount, InternalAchievement, Logger);

            Player.SendUpdatedInventory();
        }
    }
}
