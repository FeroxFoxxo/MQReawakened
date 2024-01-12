using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Players.Models.Minigames;
public class ArenaModel
{
    public bool IsMinigame { get; set; }
    public int MinigameTimeLimit { get; set; }
    public bool ActivatedSwitch { get; set; }
    public static bool ArenaActivated { get; set; }
    public static int FirstPlayerId { get; set; }
    public static int SecondPlayerId { get; set; }
    public static int ThirdPlayerId { get; set; }
    public static int FourthPlayerId { get; set; }
    public Dictionary<string, float> BestTimeForLevel { get; set; } = [];
    public static List<Player> Participants { get; set; } = [];
    public static LootCatalogInt LootCatalog { get; set; }

    public static void SetCharacterIds(IEnumerable<Player> players)
    {
        var playersInRoom = players.ToArray();

        FirstPlayerId = playersInRoom.Length > 0 ? playersInRoom[0].CharacterId : 0;
        SecondPlayerId = playersInRoom.Length > 1 ? playersInRoom[1].CharacterId : 0;
        ThirdPlayerId = playersInRoom.Length > 2 ? playersInRoom[2].CharacterId : 0;
        FourthPlayerId = playersInRoom.Length > 3 ? playersInRoom[3].CharacterId : 0;
    }

    public static void FinishMinigame(int minigameId, Player player)
    {
        ArenaActivated = false;
        var playersInRace = Participants;

        var rdmBananaReward = new Random().Next(7, 11) * player.Character.Data.GlobalLevel;
        var xpReward = player.Character.Data.ReputationForNextLevel / 11;

        var lootedItems = GrantRandomItemLoot(LootCatalog, minigameId);
        var lootableItems = GrantLootableItems(LootCatalog, minigameId);

        var sb = new SeparatedStringBuilder('<');

        sb.Append(playersInRace.Count);
        sb.Append(rdmBananaReward);
        sb.Append(xpReward);
        sb.Append(lootedItems);
        sb.Append(lootableItems);

        foreach (var participant in playersInRace)
        {
            participant.SendXt("Mp", minigameId, sb.ToString());
            participant.SendSyncEventToPlayer(new TriggerUpdate_SyncEvent(minigameId.ToString(), participant.Room.Time, playersInRace.Count));

            participant.SendCashUpdate();
            participant.SendUpdatedInventory(false);
        }
    }

    public static string GrantRandomItemLoot(LootCatalogInt lootCatalog, int arenaId)
    {
        var random = new Random();
        var itemsGotten = new List<ItemModel>();

        foreach (var reward in lootCatalog.LootCatalog[arenaId].ItemRewards)
            foreach (var item in reward.Items)
                itemsGotten.Add(item);

        var randomItemRewardLoot = itemsGotten[random.Next(itemsGotten.Count)];

        return FormatItemString(randomItemRewardLoot);
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

    public static string FormatItemString(ItemModel item)
    {
        var sb = new SeparatedStringBuilder('{');

        sb.Append(item.ItemId);
        sb.Append(item.Count);
        sb.Append(item.BindingCount);
        sb.Append(DateTime.Now);

        return sb.ToString();
    }
}
