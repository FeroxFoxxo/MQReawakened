using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class ColorModel
{
    public const char ColorDelimiter = '&';

    public float Red { get; set; }
    public float Green { get; set; }
    public float Blue { get; set; }

    public ColorModel () {}

    public ColorModel(string serverString)
    {
        var values = serverString.Split(ColorDelimiter);

        Red = float.Parse(values[0]);
        Green = float.Parse(values[1]);
        Blue = float.Parse(values[2]);
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(ColorDelimiter);

        sb.Append(Red);
        sb.Append(Green);
        sb.Append(Blue);

        return sb.ToString();
    }
}
