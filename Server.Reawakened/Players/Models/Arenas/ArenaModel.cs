using Server.Base.Core.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Server.Reawakened.Players.Models.Arenas;

public class ArenaModel
{
    public bool ShouldStartArena { get; set; }
    public bool HasStarted { get; set; }
    public int FirstPlayerId { get; set; }
    public int SecondPlayerId { get; set; }
    public int ThirdPlayerId { get; set; }
    public int FourthPlayerId { get; set; }
    public Dictionary<string, float> BestTimeForLevel { get; set; } = [];

    public void SetCharacterIds(IEnumerable<Player> players)
    {
        var playersInGroup = players.ToArray();

        FirstPlayerId = playersInGroup.Length > 0 ? playersInGroup[0].CharacterId : 0;
        SecondPlayerId = playersInGroup.Length > 1 ? playersInGroup[1].CharacterId : 0;
        ThirdPlayerId = playersInGroup.Length > 2 ? playersInGroup[2].CharacterId : 0;
        FourthPlayerId = playersInGroup.Length > 3 ? playersInGroup[3].CharacterId : 0;
    }

    public static string GrantLootedItems(InternalLoot lootCatalog, int levelId, string arenaId)
    {
        var random = new Random();
        var itemsGotten = new List<ItemModel>();

        foreach (var reward in lootCatalog.LootCatalog[levelId][arenaId].ItemRewards)
            foreach (var item in reward.Items)
                itemsGotten.Add(item.Value);

        var randomItemReward = itemsGotten[random.Next(itemsGotten.Count)];

        var itemsLooted = randomItemReward.DeepCopy();

        return itemsLooted.ToString();
    }

    public static string GrantLootableItems(InternalLoot lootCatalog, int levelId, string arenaId)
    {
        var lootableItems = new SeparatedStringBuilder('|');

        foreach (var reward in lootCatalog.LootCatalog[levelId][arenaId].ItemRewards)
            foreach (var itemReward in reward.Items)
                lootableItems.Append(itemReward.Value.ItemId);

        return lootableItems.ToString();
    }
}
