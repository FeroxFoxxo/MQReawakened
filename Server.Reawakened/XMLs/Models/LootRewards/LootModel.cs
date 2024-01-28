namespace Server.Reawakened.XMLs.Models.LootRewards;

public class LootModel(string objectId, List<BananaReward> bananaRewards, List<ItemReward> itemRewards, bool doWheel, int weightRange)
{
    public string ObjectId { get; } = objectId;
    public List<BananaReward> BananaRewards { get; } = bananaRewards;
    public List<ItemReward> ItemRewards { get; } = itemRewards;
    public bool DoWheel { get; } = doWheel;
    public int WeightRange { get; } = weightRange;
}
