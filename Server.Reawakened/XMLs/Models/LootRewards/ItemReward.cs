using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.XMLs.Models.LootRewards;

public class ItemReward(List<KeyValuePair<int, ItemModel>> items, int rewardAmount)
{
    public List<KeyValuePair<int, ItemModel>> Items => items;
    public int RewardAmount => rewardAmount;
}
