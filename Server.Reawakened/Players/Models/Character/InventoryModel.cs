using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class InventoryModel
{
    public Dictionary<int, ItemModel> Items { get; set; }

    public InventoryModel() => Items = new Dictionary<int, ItemModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('>');

        foreach (var item in Items)
            sb.Append(GetItem(item));

        return sb.ToString();
    }

    public static string GetItem(KeyValuePair<int, ItemModel> item)
    {
        var sb = new SeparatedStringBuilder('{');

        sb.Append(item.Key);
        sb.Append(item.Value);

        return sb.ToString();
    }
}
