using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Protocol;

public class LevelUpDataModel
{
    public int Level { get; set; }
    public int IncLife { get; set; }
    public int IncDefense { get; set; }
    public int IncResistance { get; set; }
    public int IncPowerJewel { get; set; }
    public int IncAbilityPower { get; set; }
    public int CurrentLevelCompletion { get; set; }
    public int ItemId { get; set; }
    public int Nc { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('<');

        sb.Append(Level);
        sb.Append(IncLife);
        sb.Append(IncDefense);
        sb.Append(IncResistance);
        sb.Append(IncPowerJewel);
        sb.Append(IncAbilityPower);
        sb.Append(CurrentLevelCompletion);
        sb.Append(ItemId);
        sb.Append(Nc);

        return sb.ToString();
    }
}
