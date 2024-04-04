namespace Server.Reawakened.XMLs.Models.LootRewards;

public class LootModel(string objectId, List<BananaReward> bananaRewards, List<ItemReward> itemRewards, bool doWheel, int multiplayerWheelChance, int weightRange)
{
    public string ObjectId => objectId;
    public List<BananaReward> BananaRewards => bananaRewards;
    public List<ItemReward> ItemRewards => itemRewards;
    public bool DoWheel => doWheel;
    public int MultiplayerWheelChance => multiplayerWheelChance;
    public int WeightRange => weightRange;
}
