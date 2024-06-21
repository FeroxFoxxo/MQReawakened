using A2m.Server;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterCustomDataModel(CharacterDbEntry entry)
{
    public int CharacterId => entry.Id;
    public Dictionary<CustomDataProperties, int> Properties => entry.Properties;
    public Dictionary<CustomDataProperties, ColorModel> Colors => entry.Colors;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(':');
        sb.Append(CharacterId);

        foreach (var property in Properties)
            sb.Append(BuildProperty(property));

        return sb.ToString();
    }

    private string BuildProperty(KeyValuePair<CustomDataProperties, int> property)
    {
        var sb = new SeparatedStringBuilder('=');

        sb.Append((int)property.Key);
        sb.Append(property.Value);
        sb.Append(Colors[property.Key]);

        return sb.ToString();
    }
}
