using A2m.Server;
using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class CharacterCustomDataModel
{
    public const char KeyValueDelimiter = '=';
    public const char PropertyDelimiter = ':';

    public int CharacterId { get; set; }
    public Dictionary<CustomDataProperties, int> Properties { get; set; }
    public Dictionary<CustomDataProperties, ColorModel> Colors { get; set; }

    public CharacterCustomDataModel()
    {
        Properties = new Dictionary<CustomDataProperties, int>();
        Colors = new Dictionary<CustomDataProperties, ColorModel>();
    }

    public CharacterCustomDataModel(string serverString)
    {
        Properties = new Dictionary<CustomDataProperties, int>();
        Colors = new Dictionary<CustomDataProperties, ColorModel>();

        var properties = serverString.Split(PropertyDelimiter);

        foreach (var prop in properties)
        {
            var values = prop.Split(KeyValueDelimiter);

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
        var sb = new SeparatedStringBuilder(PropertyDelimiter);
        sb.Append(CharacterId);

        foreach (var property in Properties)
            sb.Append(BuildProperty(property));

        return sb.ToString();
    }

    private string BuildProperty(KeyValuePair<CustomDataProperties, int> property)
    {
        var sb = new SeparatedStringBuilder(KeyValueDelimiter);

        sb.Append((int) property.Key);
        sb.Append(property.Value);
        sb.Append(Colors[property.Key]);

        return sb.ToString();
    }
}
