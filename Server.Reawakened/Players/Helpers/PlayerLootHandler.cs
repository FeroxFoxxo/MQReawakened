using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.LootRewards;

namespace Server.Reawakened.Players.Helpers;

public static class PlayerLootHandler
{
    public static void GrantLoot(this Player player, string gameObjectId, InternalLoot lootCatalog,
        ItemCatalog itemCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var loot = lootCatalog.GetLootById(gameObjectId);

        if (string.IsNullOrEmpty(loot.ObjectId))
            logger.LogError("Loot table not yet implemented for chest with ID '{ChestId}'.", gameObjectId);

        if (loot.BananaRewards.Count > 0)
            loot.BananaRewards.GrantLootBananas(player);

        if (loot.ItemRewards.Count > 0)
            loot.GrantLootItems(gameObjectId, player, itemCatalog);
    }

    private static void GrantLootBananas(this List<BananaReward> bananas, Player player)
    {
        var random = new Random();

        var totalBananas = 0;

        foreach (var banana in bananas)
            totalBananas += random.Next(banana.BananaMin, banana.BananaMax + 1);

        player.AddBananas(totalBananas);
    }

    private static void GrantLootItems(this LootModel lootModel, string objectId, Player player,
        ItemCatalog itemCatalog)
    {
        var random = new Random();

        var itemsLooted = new SeparatedStringBuilder('|');
        var lootableItems = new SeparatedStringBuilder('|');

        var gottenItems = new List<ItemModel>();

        foreach (var itemReward in lootModel.ItemRewards)
        {
            foreach (var item in itemReward.Items)
                lootableItems.Append(item.Value.ItemId);

            var count = itemReward.RewardAmount;

            while (count > 0)
            {

                var randomWeight = random.NextInt64(1, lootModel.WeightRange);
                var selector = 0;
                foreach (var item in itemReward.Items)
                {
                    randomWeight -= item.Key;
                    if (randomWeight <= 0)
                        break;
                    else
                        selector++;
                }
                var chosenItem = itemReward.Items[selector];
                gottenItems.Add(chosenItem.Value);
                count--;
            }
        }

        foreach (var item in gottenItems)
        {
            itemsLooted.Append(item.ToString());
            if (item.ItemId > 0)
                player.AddItem(itemCatalog.GetItemFromId(item.ItemId), item.Count, itemCatalog);
        }

        if (lootModel.DoWheel)
            SendLootWheel(player, itemsLooted.ToString(), lootableItems.ToString(), objectId);

        player.SendUpdatedInventory();
    }

    private static void SendLootWheel(Player player, string itemsLooted, string lootableItems, string gameObjectId)
        => player.SendXt("iW", itemsLooted, lootableItems, gameObjectId, 0);

}
