using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class RecipeListModel
{
    public List<RecipeModel> RecipeList { get; set; }

    public RecipeListModel() =>
        RecipeList = new List<RecipeModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var recipe in RecipeList)
            sb.Append(recipe);

        return sb.ToString();
    }
}
