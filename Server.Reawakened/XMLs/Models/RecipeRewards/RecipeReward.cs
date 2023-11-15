using Server.Reawakened.Players.Models.Character;
namespace Server.Reawakened.XMLs.Models.RecipeRewards;

public class RecipeReward(List<RecipeModel> recipes, int rewardAmount)
{
    public List<RecipeModel> Recipes { get; } = recipes;
    public int RewardAmount { get; } = rewardAmount;
}
