using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.XMLs.Models;

public class LootModel(int objectId, int bananaMin, int bananaMax, List<ItemModel> items)
{
    public int ObjectId { get; } = objectId;
    public int BananaMin { get; } = bananaMin;
    public int BananaMax { get; } = bananaMax;
    public List<ItemModel> Items { get; } = items;
}
