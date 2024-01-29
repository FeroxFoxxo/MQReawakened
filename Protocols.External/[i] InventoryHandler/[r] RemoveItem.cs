using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

public class RemoveItem : ExternalProtocol
{
    public override string ProtocolName => "ir";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var itemId = int.Parse(message[5]);
        var removeCount = int.Parse(message[6]);

        var itemDescription = ItemCatalog.GetItemFromId(itemId);

        Player.RemoveItem(itemDescription, removeCount);

        Player.SendUpdatedInventory(false);
    }
}
