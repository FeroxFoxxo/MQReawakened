using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.LootHandlers;

public static class PlayerLootHandler
{
    private static Dictionary<string, dynamic> GetLootInfo(int gameObjectId, InternalLootCatalog lootCatalog) 
        => lootCatalog.GetLootById(gameObjectId);

    private static Dictionary<string, dynamic> ParseLootByType(Dictionary<string, dynamic> lootInfo)
    {
        var rewardType = lootInfo["rewardType"];
        object[] reward = lootInfo["reward"];

        var convertedLootInfo = new Dictionary<string, dynamic>(); //lootInfo.ToDictionary(entry => entry.Key,
                                               //entry => entry.Value);
        foreach (KeyValuePair<string, dynamic> pair in lootInfo)
        {
            convertedLootInfo[pair.Key] = pair.Value;
        }

        switch (rewardType)
        {
            case "Banana":
                {
                    string[] bananasReward = Array.ConvertAll(reward, x => x.ToString());
                    convertedLootInfo["reward"] = bananasReward;

                    break;
                }
            case "Item":
                {
                    int[][] items = Array.ConvertAll(reward, childArr =>
                    {
                        return Array.ConvertAll<int, int>((int[])childArr, number => Convert.ToInt32(number));
                    });
                    convertedLootInfo["reward"] = items;

                    break;
                }
        }

        return convertedLootInfo;
    }

    private static void GrantLootBananas(Player player, Dictionary<string, dynamic> lootInfo)
    { //Banana reward not properly functioning yet
        Random random = new Random();

        var bananasReward = lootInfo["reward"];

        int minBananas = Convert.ToInt32(bananasReward[0]);
        int maxBananas = Convert.ToInt32(bananasReward[1]);
        var bananasGot = random.Next(minBananas, maxBananas);

        player.AddBananas(bananasGot);
    }

    private static void SendLootWheel(Player player, string itemsLooted, string lootableItems, int gameObjectId) 
        => player.SendXt("iW", itemsLooted, lootableItems, gameObjectId, 0);

    private static void GrantLootItems(Player player, Dictionary<string, dynamic> lootInfo, int gameObjectId, ItemCatalog itemCatalog)
    {
        Random random = new Random();

        var items = lootInfo["reward"];

        var gottenItems = new int[][] { items[random.Next(items.Length)] };
        var itemsLooted = "";
        var lootableItems = "";

        foreach (var item in gottenItems)
        {
            var itemId = item[0];
            var count = item[1];
            var bindingCount = item[2];

            itemsLooted += $"{itemId}{{{count}{{{bindingCount}{{{DateTime.Now}|";
            player.Character.AddItem(itemCatalog.GetItemFromId(itemId), count);
        }

        foreach (var item in items)
        {
            lootableItems += $"{item[0]}|";
        }

        SendLootWheel(player, itemsLooted, lootableItems, gameObjectId);
        player.SendUpdatedInventory(false);
    }

    public static void GrantLoot(this Player player, int gameObjectId, InternalLootCatalog lootCatalog, ItemCatalog itemCatalog, Microsoft.Extensions.Logging.ILogger logger)
    {
        var baseLootInfo = GetLootInfo(gameObjectId, lootCatalog);
        var lootInfo = ParseLootByType(baseLootInfo);

        if (baseLootInfo["objectId"] == "0")
        {
            logger.LogError("Loot table not yet implemented for chest with ID '{chestId}'.", gameObjectId);
        }

        switch (lootInfo["rewardType"]) {
            case "Banana":
                {
                    GrantLootBananas(player, lootInfo);
                    break;
                }
            case "Item":
                {
                    GrantLootItems(player, lootInfo, gameObjectId, itemCatalog); 
                    break;
                }
        }
    }
}
