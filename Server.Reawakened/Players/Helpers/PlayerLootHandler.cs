using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Models;

namespace Server.Reawakened.Players.LootHandlers;

public static class PlayerLootHandler
{
    public static void GrantLoot(this Player player, int gameObjectId, InternalLootCatalog lootCatalog,
        ItemCatalog itemCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var loot = lootCatalog.GetLootById(gameObjectId);

        if (loot.ObjectId <= 0)
            logger.LogError("Loot table not yet implemented for chest with ID '{ChestId}'.", gameObjectId);

        if (loot.BananaMax >= 0 && loot.BananaMin >= 0)
            GrantLootBananas(player, loot);

        if (loot.Items.Count > 0)
            GrantLootItems(player, loot, itemCatalog);
    }

    private static void GrantLootBananas(Player player, LootModel lootModel)
    {
        //Banana reward not properly functioning yet
        var random = new Random();

        var bananasGot = random.Next(lootModel.BananaMin, lootModel.BananaMax + 1);

        player.AddBananas(bananasGot);
    }

    private static void GrantLootItems(Player player, LootModel lootModel, ItemCatalog itemCatalog)
    {
        var random = new Random();

        var gottenItems = new ItemModel[] { lootModel.Items[random.Next(lootModel.Items.Count)] };
        var itemsLooted = new SeparatedStringBuilder('|');
        var lootableItems = new SeparatedStringBuilder('|');

        foreach (var item in gottenItems)
        {
            itemsLooted.Append(item.ToString());
            player.Character.AddItem(itemCatalog.GetItemFromId(item.ItemId), item.Count);
        }

        foreach (var item in lootModel.Items)
            lootableItems.Append(item.ItemId);

        SendLootWheel(player, itemsLooted.ToString(), lootableItems.ToString(), lootModel.ObjectId);
        player.SendUpdatedInventory(false);
    }

    private static void SendLootWheel(Player player, string itemsLooted, string lootableItems, int gameObjectId)
        => player.SendXt("iW", itemsLooted, lootableItems, gameObjectId, 0);
}
