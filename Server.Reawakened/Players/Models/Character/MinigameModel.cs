using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.LootRewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.Players.Models.Character;
public class ArenaModel
{
    public bool StartArena { get; set; }
    public bool HasStarted { get; set; }
    public int FirstPlayerId { get; set; }
    public int SecondPlayerId { get; set; }
    public int ThirdPlayerId { get; set; }
    public int FourthPlayerId { get; set; }
    public Dictionary<string, float> BestTimeForLevel { get; set; } = [];

    public ItemCatalog ItemCatalog { get; set; }

    public void SetCharacterIds(Player player, IEnumerable<Player> players)
    {
        var playersInGroup = players.ToArray();
        player.TempData.ArenaModel.FirstPlayerId = playersInGroup.Length > 0 ? playersInGroup[0].GameObjectId : 0;
        player.TempData.ArenaModel.SecondPlayerId = playersInGroup.Length > 1 ? playersInGroup[1].GameObjectId : 0;
        player.TempData.ArenaModel.ThirdPlayerId = playersInGroup.Length > 2 ? playersInGroup[2].GameObjectId : 0;
        player.TempData.ArenaModel.FourthPlayerId = playersInGroup.Length > 3 ? playersInGroup[3].GameObjectId : 0;
    }

    public string GrantLootedItems(LootCatalogInt LootCatalog, int arenaId)
    {
        var random = new Random();
        var itemsGotten = new List<ItemModel>();

        foreach (var reward in LootCatalog.LootCatalog[arenaId].ItemRewards)
        {
            foreach (var item in reward.Items)
            {
                itemsGotten.Add(item);
            }
        }
        var randomItemReward = itemsGotten[random.Next(itemsGotten.Count)].ItemId;

        var itemsLooted = FormatItemString(randomItemReward, 1);

        Console.WriteLine(itemsLooted.ToString());
        return itemsLooted.ToString();
    }

    public string GrantLootableItems(LootCatalogInt LootCatalog, int arenaId)
    {
        var lootableItems = new SeparatedStringBuilder('|');

        if (LootCatalog.LootCatalog.ContainsKey(arenaId))
        {
            foreach (var reward in LootCatalog.LootCatalog[arenaId].ItemRewards)
                foreach (var itemReward in reward.Items)
                    lootableItems.Append(itemReward.ItemId);
        }

        return lootableItems.ToString();
    }

    public string FormatItemString(int itemId, int amount)
    {
        Console.WriteLine("ItemId: " + itemId);

        var sb = new SeparatedStringBuilder('{');

        sb.Append(itemId);
        sb.Append(amount);
        sb.Append(amount);
        sb.Append(DateTime.Now);

        return sb.ToString();
    }
}
