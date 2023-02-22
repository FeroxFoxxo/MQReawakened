using A2m.Server;
using Server.Base.Network;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtensions
{
    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem) =>
        characterData.Data.Inventory.Items.TryGetValue(itemId, out outItem);

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

    public static string GetItemListString(this InventoryModel inventory)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var item in inventory.Items)
            sb.Append(item.Value.ToString());

        return sb.ToString();
    }

    public static void SendUpdatedInventory(this CharacterModel character, NetState state, bool fromEquippedUpdate) =>
        state.SendXt("ip", character.Data.Inventory.GetItemListString(), fromEquippedUpdate);
}
