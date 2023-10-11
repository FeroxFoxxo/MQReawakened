using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

public class RemoveItem : ExternalProtocol
{
    public ItemCatalog ItemCatalog { get; set; }

    public override string ProtocolName => "ir";

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var itemId = int.Parse(message[5]);
        var removeCount = int.Parse(message[5]);

        var itemDescription = ItemCatalog.GetItemFromId(itemId);

        character.RemoveItem(itemDescription, removeCount);

        Player.SendUpdatedInventory(false);
    }
}
