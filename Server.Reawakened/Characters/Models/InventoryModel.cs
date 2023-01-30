using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class InventoryModel
{
    public const char DictSeparator = '{';
    public const char FieldSeparator = '>';

    public Dictionary<int, ItemModel> Items { get; set; }

    public InventoryModel() => Items = new Dictionary<int, ItemModel>();

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var item in Items)
            sb.Append(GetItem(item));

        return sb.ToString();
    }

    public static string GetItem(KeyValuePair<int, ItemModel> item)
    {
        var sb = new SeparatedStringBuilder(DictSeparator);

        sb.Append(item.Key);
        sb.Append(item.Value);

        return sb.ToString();
    }
}
