using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class ItemModel
{
    public int ItemId { get; set; }
    public int Count { get; set; }
    public int BindingCount { get; set; }
    public DateTime DelayUseExpiry { get; set; }
    public int Weight { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('{');

        sb.Append(ItemId);
        sb.Append(Count);
        sb.Append(BindingCount);
        sb.Append(DelayUseExpiry.Equals(DateTime.MinValue) ? 0 : DelayUseExpiry.Ticks);
        sb.Append(Weight);

        return sb.ToString();
    }
}
