namespace Server.Reawakened.XMLs.Models.LootRewards;

public class BananaReward(int bananaMin, int bananaMax)
{
    public int BananaMin { get; } = bananaMin;
    public int BananaMax { get; } = bananaMax;
}
