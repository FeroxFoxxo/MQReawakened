using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
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
        var newEquipment = new TemporaryEquipmentModel(message[5]);

        foreach (var item in newEquipment.EquippedItems)
        {
            var itemDesc = ItemCatalog.GetItemFromId(item.Value);

            if (Player.Character.Equipment.EquippedItems.TryGetValue(item.Key, out var previouslyEquippedId))
            {
                if (ItemAlreadyEquipped(item.Value, previouslyEquippedId))
                    continue;

                Player.AddItem(ItemCatalog.GetItemFromId(previouslyEquippedId), 1, ItemCatalog);
            }

            Player.RemoveItem(ItemCatalog.GetItemFromId(item.Value), 1, ItemCatalog, ItemRConfig);

            if (itemDesc != null)
                Player.CheckAchievement(AchConditionType.EquipItem, [itemDesc.PrefabName], InternalAchievement, Logger);
        }

        AddUnequippedToInventory(newEquipment);
        Player.Character.Equipment.UpdateFromTempEquip(newEquipment);

        Player.UpdateEquipment();
        Player.SendUpdatedInventory(true);
    }

    private static bool ItemAlreadyEquipped(int itemId, int previouslyEquippedId) => itemId == previouslyEquippedId;

    private void AddUnequippedToInventory(TemporaryEquipmentModel newEquipment)
    {
        foreach (var equippedItem in Player.Character.Equipment.EquippedItems)
            if (!newEquipment.EquippedItems.ContainsKey(equippedItem.Key))
                Player.AddItem(ItemCatalog.GetItemFromId(equippedItem.Value), 1, ItemCatalog);
    }
}
