using System.Text;

namespace Server.Reawakened.Characters.Models;

public class HotbarModel
{
    public const char FieldSeparator = '|';

    public Dictionary<int, ItemModel> HotbarButtons { get; set; }

    public HotbarModel() =>
        HotbarButtons = new Dictionary<int, ItemModel>();

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var button in HotbarButtons)
        {
            sb.Append(button.Key);
            sb.Append(FieldSeparator);
            sb.Append(button.Value);
            sb.Append(FieldSeparator);
        }
        return sb.ToString();
    }
}
