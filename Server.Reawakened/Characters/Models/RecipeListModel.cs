using System.Text;

namespace Server.Reawakened.Characters.Models;

public class RecipeListModel
{
    public const char FieldSeparator = '|';

    public List<RecipeModel> RecipeList { get; set; }

    public RecipeListModel() {}

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var recipe in RecipeList)
        {
            sb.Append(recipe);
            sb.Append(FieldSeparator);
        }

        return sb.ToString();
    }
}
