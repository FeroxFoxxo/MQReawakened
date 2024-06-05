using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;

namespace Protocols.External._i__InventoryHandler;

public class EquipItem : ExternalProtocol
{
    public override string ProtocolName => "ie";

    public ItemCatalog ItemCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<EquipItem> Logger { get; set; }
    public ItemRConfig ItemRConfig { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var newEquipment = new EquipmentModel(message[5]);

        foreach (var item in newEquipment.EquippedItems)
        {
            var itemDesc = ItemCatalog.GetItemFromId(item.Value);

            if (itemDesc != null)
                Player.CheckAchievement(AchConditionType.EquipItem, [itemDesc.PrefabName], InternalAchievement, Logger);

            if (character.Data.Equipment.EquippedItems.TryGetValue(item.Key, out var previouslyEquippedId))
            {
                if (ItemAlreadyEquipped(item.Value, previouslyEquippedId))
                    continue;

                Player.AddItem(ItemCatalog.GetItemFromId(previouslyEquippedId), 1, ItemCatalog);
            }

            Player.RemoveItem(ItemCatalog.GetItemFromId(item.Value), 1, ItemCatalog, ItemRConfig);
        }

        AddUnequippedToInventory(newEquipment);
        Player.Character.Data.Equipment = newEquipment;

        Player.UpdateEquipment();
        Player.SendUpdatedInventory(true);
    }

    private bool ItemAlreadyEquipped(int itemId, int previouslyEquippedId) => itemId == previouslyEquippedId;

    private void AddUnequippedToInventory(EquipmentModel newEquipment)
    {
        var character = Player.Character;

        foreach (var equippedItem in character.Data.Equipment.EquippedItems)
            if (!newEquipment.EquippedItems.ContainsKey(equippedItem.Key))
                Player.AddItem(ItemCatalog.GetItemFromId(equippedItem.Value), 1, ItemCatalog);
    }
}
