using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Models.LootRewards;

namespace Server.Reawakened.Players.LootHandlers;

public static class PlayerLootHandler
{
    public static void GrantLoot(this Player player, int gameObjectId, InternalLootCatalog lootCatalog,
        ItemCatalog itemCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var loot = lootCatalog.GetLootById(gameObjectId);

        if (loot.ObjectId <= 0)
            logger.LogError("Loot table not yet implemented for chest with ID '{ChestId}'.", gameObjectId);

        if (loot.BananaRewards.Count > 0)
            loot.BananaRewards.GrantLootBananas(player);

        if (loot.ItemRewards.Count > 0)
            loot.ItemRewards.GrantLootItems(gameObjectId, player, itemCatalog);
    }

    private static void GrantLootBananas(this List<BananaReward> bananas, Player player)
    {
        var random = new Random();

        var totalBananas = 0;

        foreach (var banana in bananas)
            totalBananas += random.Next(banana.BananaMin, banana.BananaMax + 1);

        player.AddBananas(totalBananas);
    }

    private static void GrantLootItems(this List<ItemReward> items, int objectId, Player player, ItemCatalog itemCatalog)
    {
        var random = new Random();

        var itemsLooted = new SeparatedStringBuilder('|');
        var lootableItems = new SeparatedStringBuilder('|');

        var gottenItems = new List<ItemModel>();

        foreach (var itemReward in items)
        {
            foreach (var item in itemReward.Items)
                lootableItems.Append(item.ItemId);

            var count = itemReward.RewardAmount;

            while (count > 0)
            {
                var chosenItem = itemReward.Items[random.Next(itemReward.Items.Count)];
                gottenItems.Add(chosenItem);
                count--;
            }
        }

        foreach (var item in gottenItems)
        {
            itemsLooted.Append(item.ToString());
            player.Character.AddItem(itemCatalog.GetItemFromId(item.ItemId), item.Count);
        }

        SendLootWheel(player, itemsLooted.ToString(), lootableItems.ToString(), objectId);
        player.SendUpdatedInventory(false);
    }

    private static void SendLootWheel(Player player, string itemsLooted, string lootableItems, int gameObjectId)
        => player.SendXt("iW", itemsLooted, lootableItems, gameObjectId, 0);
}
