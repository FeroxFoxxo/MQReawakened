using A2m.Server;
using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class TribeDataModel
{
    public const char FieldSeparator = '|';

    public TribeType TribeType { get; set; }
    public int BadgePoints { get; set; }
    public bool Unlocked { get; set; }

    public TribeDataModel() {}

    public TribeDataModel(string serverData)
    {
        var inputValues = serverData.Split(FieldSeparator);
        TribeType = (TribeType) int.Parse(inputValues[0]);
        BadgePoints = int.Parse(inputValues[1]);
        Unlocked = inputValues[2] == "1";
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        sb.Append((int)TribeType);
        sb.Append(BadgePoints);
        sb.Append(Unlocked ? 1 : 0);

        return sb.ToString();
    }
}
