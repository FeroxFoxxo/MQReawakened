using A2m.Server;
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
        ItemCatalog itemCatalog, InternalAchievement internalAchievement, Microsoft.Extensions.Logging.ILogger logger)
    {
        var loot = lootCatalog.GetLootById(player.Room.LevelInfo.LevelId, gameObjectId);

        if (string.IsNullOrEmpty(loot.ObjectId))
            logger.LogError("Loot table not yet implemented for chest with ID '{ChestId}'.", gameObjectId);

        if (loot.BananaRewards.Count > 0)
            loot.BananaRewards.GrantLootBananas(player, internalAchievement, logger);

        if (loot.ItemRewards.Count > 0)
            loot.GrantLootItems(gameObjectId, player, itemCatalog);
    }

    private static void GrantLootBananas(this List<BananaReward> bananas, Player player, InternalAchievement internalAchievement, Microsoft.Extensions.Logging.ILogger logger)
    {
        var random = new Random();

        var totalBananas = 0;

        foreach (var banana in bananas)
            totalBananas += random.Next(banana.BananaMin, banana.BananaMax + 1);

        player.AddBananas(totalBananas, internalAchievement, logger);
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

        if (lootModel.MultiplayerWheelChance > 0 && player.Room.Players.Count > 1 && player.Room.LevelInfo.Type == LevelType.Trail)
        {
            var randomChance = new Random().Next(100);

            if (randomChance < lootModel.MultiplayerWheelChance)
            {
                var multiplayerWheelData = new SeparatedStringBuilder('|');

                multiplayerWheelData.Append(objectId);
                multiplayerWheelData.Append(gottenItems[new Random().Next(gottenItems.Count)].ItemId);
                multiplayerWheelData.Append(lootableItems);

                foreach (var groupMember in player.Room.Players.Values)
                    SendMultiplayerLootWheel(groupMember, multiplayerWheelData.ToString());
            }
        }

        if (lootModel.DoWheel && player.Room.Players.Count <= 1)
            SendLootWheel(player, itemsLooted.ToString(), lootableItems.ToString(), objectId);

        player.SendUpdatedInventory();
    }

    public static void GrantDynamicLoot(this Player player, int level, EnemyDropModel drop, ItemCatalog itemCatalog)
    {
        var random = new Random();
        float chance;
        var finalItemId = 0;

        chance = (float)random.NextDouble();
        if (chance <= drop.Chance)
        {
            switch (drop.Type)
            {
                case Entities.Enums.DynamicDropType.Item:
                    finalItemId = drop.Id;
                    break;
                case Entities.Enums.DynamicDropType.RandomArmor:
                    //Magic number 4 here will be changed and sent to config once I get more info on clothing drops
                    var armorList = itemCatalog.GetItemsFromLevel(level - 4, level + 4, A2m.Server.ItemCategory.Wearable);
                    finalItemId = armorList[random.Next(armorList.Count)].ItemId;
                    break;
            }
        }

        if (finalItemId > 0)
            player.AddItem(itemCatalog.GetItemFromId(finalItemId), 1, itemCatalog);

        player.SendUpdatedInventory(false);
    }

    private static void SendLootWheel(Player player, string itemsLooted, string lootableItems, string gameObjectId)
        => player.SendXt("iW", itemsLooted, lootableItems, gameObjectId, 0);

    private static void SendMultiplayerLootWheel(Player player, string lootData)
        => player.SendXt("js", lootData);
}
