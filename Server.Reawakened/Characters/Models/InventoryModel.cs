using System.Text;

namespace Server.Reawakened.Characters.Models;

public class InventoryModel
{
    public const char DictSeparator = '{';
    public const char FieldSeparator = '>';

    public Dictionary<int, ItemModel> Items { get; set; }

    public InventoryModel() {}

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var item in Items)
        {
            sb.Append(item.Key);
            sb.Append(DictSeparator);
            sb.Append(item.Value);
            sb.Append(FieldSeparator);
        }
        return sb.ToString();
    }
}
