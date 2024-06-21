using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterResistancesModel(CharacterDbEntry entry)
{
    public int InternalDamageResistPointsStandard => entry.InternalDamageResistPointsStandard;
    public int InternalDamageResistPointsFire => entry.InternalDamageResistPointsFire;
    public int InternalDamageResistPointsIce => entry.InternalDamageResistPointsIce;
    public int InternalDamageResistPointsPoison => entry.InternalDamageResistPointsPoison;
    public int InternalDamageResistPointsLightning => entry.InternalDamageResistPointsLightning;

    public int ExternalDamageResistPointsStandard => entry.ExternalDamageResistPointsStandard;
    public int ExternalDamageResistPointsFire => entry.ExternalDamageResistPointsFire;
    public int ExternalDamageResistPointsIce => entry.ExternalDamageResistPointsIce;
    public int ExternalDamageResistPointsPoison => entry.ExternalDamageResistPointsPoison;
    public int ExternalDamageResistPointsLightning => entry.ExternalDamageResistPointsLightning;

    public int ExternalStatusEffectResistSecondsStun => entry.ExternalStatusEffectResistSecondsStun;
    public int ExternalStatusEffectResistSecondsSlow => entry.ExternalStatusEffectResistSecondsSlow;
    public int ExternalStatusEffectResistSecondsFreeze => entry.ExternalStatusEffectResistSecondsFreeze;

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
