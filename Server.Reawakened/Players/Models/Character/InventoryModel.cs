using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class InventoryModel(CharacterDbEntry entry)
{
    public Dictionary<int, ItemModel> Items => entry.Items;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('>');

        foreach (var item in Items)
            sb.Append(item.Value);

        return sb.ToString();
    }
}
