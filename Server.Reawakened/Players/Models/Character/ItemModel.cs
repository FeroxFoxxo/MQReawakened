using A2m.Server;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class ItemModel
{
    public int ItemId { get; set; }
    public int Count { get; set; }
    public int BindingCount { get; set; }

    public DateTime DelayUseExpiry = DateTime.Now;

    public ItemModel()
    {
    }

    public ItemModel(int itemId, int count, int bindingCount, DateTime delayUseExpiry)
    {
        ItemId = itemId;
        Count = count;
        BindingCount = bindingCount;
        DelayUseExpiry = delayUseExpiry;
    }

    public ItemModel(ItemDescription item)
    {
        ItemId = item.ItemId;
        Count = 1;
        BindingCount = item.BindingCount;
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('{');

        sb.Append(ItemId);
        sb.Append(Count);
        sb.Append(BindingCount);
        sb.Append(DelayUseExpiry.Equals(DateTime.MinValue) ? 0 : DelayUseExpiry.Ticks);

        return sb.ToString();
    }
}
