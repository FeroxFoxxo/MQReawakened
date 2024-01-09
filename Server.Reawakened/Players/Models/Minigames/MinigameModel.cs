using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Players.Models.Minigames;
public class ArenaModel
{
    public bool ShouldStartArena { get; set; }
    public bool HasStarted { get; set; }
    public int FirstPlayerId { get; set; }
    public int SecondPlayerId { get; set; }
    public int ThirdPlayerId { get; set; }
    public int FourthPlayerId { get; set; }
    public Dictionary<string, float> BestTimeForLevel { get; set; } = [];

    public static void SetCharacterIds(Player player, IEnumerable<Player> players)
    {
        var playersInGroup = players.ToArray();

        player.TempData.ArenaModel.FirstPlayerId = playersInGroup.Length > 0 ? playersInGroup[0].CharacterId : 0;
        player.TempData.ArenaModel.SecondPlayerId = playersInGroup.Length > 1 ? playersInGroup[1].CharacterId : 0;
        player.TempData.ArenaModel.ThirdPlayerId = playersInGroup.Length > 2 ? playersInGroup[2].CharacterId : 0;
        player.TempData.ArenaModel.FourthPlayerId = playersInGroup.Length > 3 ? playersInGroup[3].CharacterId : 0;
    }

    public static string GrantLootedItems(LootCatalogInt LootCatalog, int arenaId)
    {
        var random = new Random();
        var itemsGotten = new List<ItemModel>();

        foreach (var reward in LootCatalog.LootCatalog[arenaId].ItemRewards)
            foreach (var item in reward.Items)
                itemsGotten.Add(item);

        var randomItemReward = itemsGotten[random.Next(itemsGotten.Count)].ItemId;
        var itemsLooted = FormatItemString(randomItemReward, 1);

        return itemsLooted.ToString();
    }

    public static string GrantLootableItems(LootCatalogInt LootCatalog, int arenaId)
    {
        var lootableItems = new SeparatedStringBuilder('|');

        if (LootCatalog.LootCatalog.TryGetValue(arenaId, out var value))
            foreach (var reward in value.ItemRewards)
                foreach (var itemReward in reward.Items)
                    lootableItems.Append(itemReward.ItemId);

        return lootableItems.ToString();
    }

    public static string FormatItemString(int itemId, int amount)
    {
        var sb = new SeparatedStringBuilder('{');

        sb.Append(itemId);
        sb.Append(amount);
        sb.Append(amount);
        sb.Append(DateTime.Now);

        return sb.ToString();
    }
}
