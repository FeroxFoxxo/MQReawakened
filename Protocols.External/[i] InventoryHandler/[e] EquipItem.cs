using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._i__InventoryHandler;

public class EquipItem : ExternalProtocol
{
    public override string ProtocolName => "ie";

    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<EquipItem> Logger { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var newEquipment = new EquipmentModel(message[5]);

        foreach (var item in newEquipment.EquippedItems)
        {
            if (character.Data.Equipment.EquippedItems.TryGetValue(item.Key, out var previouslyEquipped))
                Player.AddItem(ItemCatalog.GetItemFromId(previouslyEquipped), 1, ServerRConfig);


            var itemDesc = ItemCatalog.GetItemFromId(item.Value);

            if (itemDesc != null)
                Player.CheckAchievement(AchConditionType.EquipItem, itemDesc.PrefabName, Logger);

            Player.RemoveItem(ItemCatalog.GetItemFromId(item.Value), 1);
        }

        character.Data.Equipment = newEquipment;

        Player.UpdateEquipment();
        Player.SendUpdatedInventory(true);
    }
}
