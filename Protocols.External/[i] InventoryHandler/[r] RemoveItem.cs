using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._i__InventoryHandler;

public class RemoveItem : ExternalProtocol
{
    public override string ProtocolName => "ir";

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var itemId = int.Parse(message[5]);
        var removeCount = int.Parse(message[5]);

        character.Data.Inventory.Items[itemId].Count -= removeCount;

        Player.SendUpdatedInventory(false);
    }
}
