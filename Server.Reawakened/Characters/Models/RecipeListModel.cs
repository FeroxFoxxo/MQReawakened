using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class RecipeListModel
{
    public const char FieldSeparator = '|';

    public List<RecipeModel> RecipeList { get; set; }

    public RecipeListModel() =>
        RecipeList = new List<RecipeModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var recipe in RecipeList)
            sb.Append(recipe);

        return sb.ToString();
    }
}
