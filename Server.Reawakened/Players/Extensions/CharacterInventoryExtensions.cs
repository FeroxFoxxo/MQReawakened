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

    public static bool TryGetKit(this CharacterModel characterData, int itemId1, int itemId2, int itemId3, int itemId4,
        int itemId5, int itemId6, int itemId7, int itemId8, int itemId9, int itemId10, out ItemModel outItem) =>
           characterData.Data.Inventory.Items.TryGetValue(itemId1, out outItem) && characterData.Data.Inventory.Items.TryGetValue(itemId2, out outItem)
        && characterData.Data.Inventory.Items.TryGetValue(itemId3, out outItem) && characterData.Data.Inventory.Items.TryGetValue(itemId4, out outItem)
        && characterData.Data.Inventory.Items.TryGetValue(itemId5, out outItem) && characterData.Data.Inventory.Items.TryGetValue(itemId6, out outItem)
        && characterData.Data.Inventory.Items.TryGetValue(itemId7, out outItem) && characterData.Data.Inventory.Items.TryGetValue(itemId8, out outItem)
        && characterData.Data.Inventory.Items.TryGetValue(itemId9, out outItem) && characterData.Data.Inventory.Items.TryGetValue(itemId10, out outItem);

    public static void RemoveItem(this CharacterModel characterData, int itemId, int count)
    {
        if (!characterData.TryGetItem(itemId, out var gottenItem))
            return;

        gottenItem.Count -= count;

        if (gottenItem.Count <= 0)
            characterData.Data.Inventory.Items.Remove(itemId);
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

    public static void SendUpdatedInventory(this Player player, bool fromEquippedUpdate) =>
        player.SendXt(
            "ip",
            player.Character.Data.Inventory.GetItemListString(),
            fromEquippedUpdate
        );    

}
