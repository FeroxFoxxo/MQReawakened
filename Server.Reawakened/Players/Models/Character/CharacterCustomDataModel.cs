using A2m.Server;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterCustomDataModel
{
    public int CharacterId { get; set; }
    public Dictionary<CustomDataProperties, int> Properties { get; set; }
    public Dictionary<CustomDataProperties, ColorModel> Colors { get; set; }

    public CharacterCustomDataModel()
    {
        Properties = [];
        Colors = [];
    }

    public CharacterCustomDataModel(string serverString)
    {
        Properties = [];
        Colors = [];

        var properties = serverString.Split(':');

        foreach (var prop in properties)
        {
            var values = prop.Split('=');

            if (values.Length != 3)
                continue;

            var key = (CustomDataProperties)int.Parse(values[0]);
            var value = int.Parse(values[1]);
            var color = new ColorModel(values[2]);

            Properties.Add(key, value);
            Colors.Add(key, color);
        }
    }

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
