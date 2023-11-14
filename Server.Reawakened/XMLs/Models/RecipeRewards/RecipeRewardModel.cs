using Server.Reawakened.XMLs.Models.RecipeRewards;

public class RecipeRewardModel(int recipeId, List<RecipeReward> recipeRewards)
{
    public int RecipeId { get; } = recipeId;
    public List<RecipeReward> RecipeRewards { get; } = recipeRewards;
}
