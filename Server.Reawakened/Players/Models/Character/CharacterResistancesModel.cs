using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterResistancesModel
{
    public int InternalDamageResistPointsStandard { get; set; }
    public int InternalDamageResistPointsFire { get; set; }
    public int InternalDamageResistPointsIce { get; set; }
    public int InternalDamageResistPointsPoison { get; set; }
    public int InternalDamageResistPointsLightning { get; set; }

    public int ExternalDamageResistPointsStandard { get; set; }
    public int ExternalDamageResistPointsFire { get; set; }
    public int ExternalDamageResistPointsIce { get; set; }
    public int ExternalDamageResistPointsPoison { get; set; }
    public int ExternalDamageResistPointsLightning { get; set; }

    public int ExternalStatusEffectResistSecondsStun { get; set; }
    public int ExternalStatusEffectResistSecondsSlow { get; set; }
    public int ExternalStatusEffectResistSecondsFreeze { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(InternalDamageResistPointsStandard);
        sb.Append(InternalDamageResistPointsFire);
        sb.Append(InternalDamageResistPointsIce);
        sb.Append(InternalDamageResistPointsPoison);
        sb.Append(InternalDamageResistPointsLightning);

        sb.Append(ExternalDamageResistPointsStandard);
        sb.Append(ExternalDamageResistPointsFire);
        sb.Append(ExternalDamageResistPointsIce);
        sb.Append(ExternalDamageResistPointsPoison);
        sb.Append(ExternalDamageResistPointsLightning);

        sb.Append(ExternalStatusEffectResistSecondsStun);
        sb.Append(ExternalStatusEffectResistSecondsSlow);
        sb.Append(ExternalStatusEffectResistSecondsFreeze);

        return sb.ToString();
    }
}
