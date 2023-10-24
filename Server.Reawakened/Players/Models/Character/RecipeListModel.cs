using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class RecipeListModel
{
    public List<RecipeModel> RecipeList { get; set; }

    public RecipeListModel() =>
        RecipeList = [];

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var recipe in RecipeList)
            sb.Append(recipe);

        return sb.ToString();
    }
}
