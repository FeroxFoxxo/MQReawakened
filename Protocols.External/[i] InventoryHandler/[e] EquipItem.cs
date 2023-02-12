using A2m.Server;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

public class EquipItem : ExternalProtocol
{
    public override string ProtocolName => "ie";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var newEquipment = new EquipmentModel(message[5]);

        foreach (var item in newEquipment.EquippedItems)
        {
            if (character.Data.Equipment.EquippedItems.TryGetValue(item.Key, out var equippedItemId))
                character.AddItem(ItemCatalog.GetItemFromId(equippedItemId), 1);

            character.RemoveItem(item.Value, 1);
        }

        character.Data.Equipment = newEquipment;

        SendXt("iq", character.Data.Equipment);
        SendXt("ip", character.Data.Inventory.GetItemListString(), true);
    }
}
