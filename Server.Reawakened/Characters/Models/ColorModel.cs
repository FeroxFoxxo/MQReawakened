using System.Text;

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
        var sb = new StringBuilder();
        sb.Append(Red);
        sb.Append(ColorDelimiter);
        sb.Append(Green);
        sb.Append(ColorDelimiter);
        sb.Append(Blue);
        return sb.ToString();
    }
}
