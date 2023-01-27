using System.Text;

namespace Server.Reawakened.Characters.Models;

public class RecipeModel
{
    public const char FieldSeparator = ',';

    public int RecipeId { get; set; }
    public int ItemId { get; set; }
    public List<IngredientModel> Ingredients { get; set; }

    public RecipeModel() =>
        Ingredients = new List<IngredientModel>();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(RecipeId);
        sb.Append(FieldSeparator);
        sb.Append(ItemId);
        sb.Append(FieldSeparator);
        sb.Append(Ingredients.Count);
        foreach (var ingredient in Ingredients)
        {
            sb.Append(FieldSeparator);
            sb.Append(ingredient);
        }
        return sb.ToString();
    }
}
