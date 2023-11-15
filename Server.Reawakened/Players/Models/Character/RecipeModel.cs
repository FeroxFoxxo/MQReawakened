using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class RecipeModel()
{
    public int RecipeId { get; set; }
    public int ItemId { get; set; }
    public List<IngredientModel> Ingredients { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(',');

        sb.Append(RecipeId);
        sb.Append(ItemId);
        sb.Append(Ingredients.Count);

        foreach (var ingredient in Ingredients)
            sb.Append(ingredient);

        return sb.ToString();
    }
}
