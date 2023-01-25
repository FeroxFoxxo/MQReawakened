using System.Globalization;

namespace Server.Reawakened.Characters.Models;

public class Color
{
    private static char COLOR_DELIMITER = '&';

    public float Red { get; set; }
    public float Green { get; set; }
    public float Blue { get; set; }

    public Color(string str)
    {
        var array = str.Split(COLOR_DELIMITER);
        Red = float.Parse(array[0], CultureInfo.InvariantCulture);
        Green = float.Parse(array[1], CultureInfo.InvariantCulture);
        Blue = float.Parse(array[2], CultureInfo.InvariantCulture);
    }

    public override string ToString()
    {
        var empty = string.Empty;
        empty += Red.ToString(CultureInfo.InvariantCulture);
        empty += COLOR_DELIMITER;
        empty += Green.ToString(CultureInfo.InvariantCulture);
        empty += COLOR_DELIMITER;
        return empty + Blue.ToString(CultureInfo.InvariantCulture);
    }
}
