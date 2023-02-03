using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class ColorModel
{
    public float Red { get; set; }
    public float Green { get; set; }
    public float Blue { get; set; }

    public ColorModel()
    {
    }

    public ColorModel(string serverString)
    {
        var values = serverString.Split('&');

        Red = float.Parse(values[0]);
        Green = float.Parse(values[1]);
        Blue = float.Parse(values[2]);
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('&');

        sb.Append(Red);
        sb.Append(Green);
        sb.Append(Blue);

        return sb.ToString();
    }
}
