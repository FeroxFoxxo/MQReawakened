using A2m.Server;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class TribeDataModel
{
    public TribeType TribeType { get; set; }
    public int BadgePoints { get; set; }
    public bool Unlocked { get; set; }

    public TribeDataModel() { }

    public TribeDataModel(string serverData)
    {
        var inputValues = serverData.Split('|');
        TribeType = (TribeType)int.Parse(inputValues[0]);
        BadgePoints = int.Parse(inputValues[1]);
        Unlocked = inputValues[2] == "1";
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append((int)TribeType);
        sb.Append(BadgePoints);
        sb.Append(Unlocked ? 1 : 0);

        return sb.ToString();
    }
}
