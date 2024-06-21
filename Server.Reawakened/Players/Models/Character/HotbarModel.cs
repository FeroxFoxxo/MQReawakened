using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class HotbarModel(CharacterDbEntry entry)
{
    public Dictionary<int, ItemModel> HotbarButtons => entry.HotbarButtons;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var button in HotbarButtons)
        {
            sb.Append(button.Key);
            sb.Append(button.Value);
        }

        return sb.ToString();
    }
}
