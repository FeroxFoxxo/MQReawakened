using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A2m.Server;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterInventoryExtentions
{
    public static bool TryGetItem(this CharacterModel characterData, int itemId, out ItemModel outItem)
    {
        if (characterData.Data.Inventory.Items.TryGetValue(itemId, out outItem))
            return true;

        outItem = null;
        return false;
    }

    public static void RemoveItem(this CharacterModel characterData, int itemId, int count)
    {
        if (characterData.TryGetItem(itemId, out var gottenItem))
        {
            gottenItem.Count -= count;
            if (gottenItem.Count <= 0)
            {
                characterData.Data.Inventory.Items.Remove(itemId);
            }
        }
    }

    public static void AddItem(this CharacterModel characterData, ItemDescription item, int count)
    {
        if (characterData.TryGetItem(item.ItemId, out var gottenItem))
        {
            gottenItem.Count += count;
        }
        else characterData.Data.Inventory.Items.Add(item.ItemId, new ItemModel()
        {
            ItemId = item.ItemId,
            Count = count,
            BindingCount = 0,
            DelayUseExpiry = DateTime.MinValue
        });
    }
}
