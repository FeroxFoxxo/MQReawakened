using System.Text;

namespace Server.Reawakened.Characters.Models;

public class ItemModel
{
    public const char FieldSeparator = '{';

    public int ItemId { get; set; }
    public int Count { get; set; }
    public int BindingCount { get; set; }
    public DateTime DelayUseExpiry { get; set; }

    public ItemModel() {}

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(ItemId);
        sb.Append(FieldSeparator);
        sb.Append(Count);
        sb.Append(FieldSeparator);
        sb.Append(BindingCount);
        sb.Append(FieldSeparator);
        sb.Append(DelayUseExpiry.Equals(DateTime.MinValue) ? "0" : DelayUseExpiry.Ticks.ToString());
        return sb.ToString();
    }
}
