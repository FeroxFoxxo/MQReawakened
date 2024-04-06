using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.XMLs.Data.LootRewards.Models;

public class ItemReward(List<KeyValuePair<int, ItemModel>> items, int rewardAmount)
{
    public List<KeyValuePair<int, ItemModel>> Items => items;
    public int RewardAmount => rewardAmount;
}
