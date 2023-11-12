using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.XMLs.Models.LootRewards;

public class ItemReward(List<ItemModel> items, int rewardAmount)
{
    public List<ItemModel> Items { get; } = items;
    public int RewardAmount { get; } = rewardAmount;
}
