using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

public class EquipItem : ExternalProtocol
{
    public override string ProtocolName => "ie";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var newEquipment = new EquipmentModel(message[5]);

        foreach (var item in newEquipment.EquippedItems)
        {
            if (character.Data.Equipment.EquippedItems.TryGetValue(item.Key, out var previouslyEquipped))
                character.AddItem(ItemCatalog.GetItemFromId(previouslyEquipped), 1);

            character.RemoveItem(ItemCatalog.GetItemFromId(item.Value), 1);
        }

        character.Data.Equipment = newEquipment;

        Player.UpdateEquipment();
        Player.SendUpdatedInventory(true);
    }
}
