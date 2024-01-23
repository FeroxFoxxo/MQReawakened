using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtensions
{
    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem) =>
        characterData.Data.Inventory.Items.TryGetValue(itemId, out outItem);

    public static void RemoveItem(this Player player, ItemDescription item, int count)
    {
        var characterData = player.Character;

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count -= count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId.ToString(), item.PrefabName, gottenItem.Count);
    }

    public static void AddItem(this Player player, ItemDescription item, int count)
    {
        var characterData = player.Character;

        if (!characterData.Data.Inventory.Items.ContainsKey(item.ItemId))
            characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
            {
                ItemId = item.ItemId,
                Count = 0,
                BindingCount = item.BindingCount,
                DelayUseExpiry = DateTime.MinValue
            });

        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        gottenItem.Count += count;

        player.CheckObjective(ObjectiveEnum.Inventorycheck, gottenItem.ItemId.ToString(), item.PrefabName, gottenItem.Count);
    }

    public static void AddKit(this CharacterModel characterData, List<ItemDescription> items, int count)
    {
        foreach (var item in items)
        {
            if (item != null)
                if (characterData.Data.Inventory.Items.TryGetValue(item.ItemId, out var gottenKit))
                    gottenKit.Count += count;
                else
                    characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
                    {
                        ItemId = item.ItemId,
                        Count = count,
                        BindingCount = item.BindingCount,
                        DelayUseExpiry = DateTime.MinValue
                    });
        }
    }

    public static string GetItemListString(this InventoryModel inventory)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var item in inventory.Items)
            sb.Append(item.Value.ToString());

        return sb.ToString();
    }

    public static void SendUpdatedInventory(this Player player, bool fromEquippedUpdate)
    {
        player.SendXt(
            "ip",
            player.Character.Data.Inventory.GetItemListString(),
            fromEquippedUpdate
        );

        foreach (var item in player.Character.Data.Inventory.Items)
            if (item.Value.Count <= 0)
                player.Character.Data.Inventory.Items.Remove(item.Key);
    }
}
