using Server.Reawakened.Characters.Helpers;

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
        var sb = new SeparatedStringBuilder(FieldSeparator);

        sb.Append(ItemId);
        sb.Append(Count);
        sb.Append(BindingCount);
        sb.Append(DelayUseExpiry.Equals(DateTime.MinValue) ? 0 : DelayUseExpiry.Ticks);
        
        return sb.ToString();
    }
}
