using System.Text;

namespace Server.Reawakened.Characters.Models;

public class IngredientModel
{
    public const char FieldSeparator = '*';

    public int ItemId { get; set; }
    public int Count { get; set; }

    public IngredientModel() {}

    public string GenerateIngredientString()
    {
        var sb = new StringBuilder();

        sb.Append(ItemId);
        sb.Append(FieldSeparator);
        sb.Append(Count);

        return sb.ToString();
    }

}
