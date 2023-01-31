using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class IngredientModel
{
    public int ItemId { get; set; }
    public int Count { get; set; }

    public IngredientModel() {}

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('*');

        sb.Append(ItemId);
        sb.Append(Count);

        return sb.ToString();
    }
}
