using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class RecipeListModel(CharacterDbEntry entry)
{
    public List<RecipeModel> RecipeList => entry.RecipeList;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var recipe in RecipeList)
            sb.Append(recipe);

        return sb.ToString();
    }
}
