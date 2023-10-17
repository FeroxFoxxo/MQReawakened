namespace Server.Reawakened.XMLs.Models;

public class ItemModel(int itemId, int count)
{
    public int ItemId { get; } = itemId;
    public int Count { get; } = count;
}
