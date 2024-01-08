using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtensions
{
    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem) =>
        characterData.Data.Inventory.Items.TryGetValue(itemId, out outItem);

    public static void RemoveItem(this CharacterModel characterData, ItemDescription item, int count)
    {
        if (!characterData.TryGetItem(item.ItemId, out var gottenItem))
            return;

        characterData.Data.Inventory.Items[gottenItem.ItemId].Count -= count;
    }

    public static void AddItem(this CharacterModel characterData, ItemDescription item, int count)
    {
        if (characterData.TryGetItem(item.ItemId, out var gottenItem))
            gottenItem.Count += count;
        else
            characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel
            {
                ItemId = item.ItemId,
                Count = count,
                BindingCount = item.BindingCount,
                DelayUseExpiry = DateTime.MinValue
            });
    }

    public static void AddKit(this CharacterModel characterData, List<ItemDescription> items, int count)
    {
        foreach (var item in items)
        {
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
