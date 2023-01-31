using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class HotbarModel
{
    public Dictionary<int, ItemModel> HotbarButtons { get; set; }

    public HotbarModel() =>
        HotbarButtons = new Dictionary<int, ItemModel>();

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
