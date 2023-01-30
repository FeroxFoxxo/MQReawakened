using Server.Reawakened.Characters.Helpers;

namespace Server.Reawakened.Characters.Models;

public class CharacterResistancesModel
{
    public const char DataDelimiter = '|';

    public int StandardDamageResistPointsInt { get; set; }
    public int FireDamageResistPointsInt { get; set; }
    public int IceDamageResistPointsInt { get; set; }
    public int PoisonDamageResistPointsInt { get; set; }
    public int LightningDamageResistPointsInt { get; set; }
    public int StandardDamageResistPointsExt { get; set; }
    public int FireDamageResistPointsExt { get; set; }
    public int IceDamageResistPointsExt { get; set; }
    public int PoisonDamageResistPointsExt { get; set; }
    public int LightningDamageResistPointsExt { get; set; }
    public int StunStatusEffectResistSecsExt { get; set; }
    public int SlowStatusEffectResistSecsExt { get; set; }
    public int FreezeStatusEffectResistSecsExt { get; set; }

    public CharacterResistancesModel() { }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(DataDelimiter);

        sb.Append(StandardDamageResistPointsInt);
        sb.Append(FireDamageResistPointsInt);
        sb.Append(IceDamageResistPointsInt);
        sb.Append(PoisonDamageResistPointsInt);
        sb.Append(LightningDamageResistPointsInt);
        sb.Append(StandardDamageResistPointsExt);
        sb.Append(FireDamageResistPointsExt);
        sb.Append(IceDamageResistPointsExt);
        sb.Append(PoisonDamageResistPointsExt);
        sb.Append(LightningDamageResistPointsExt);
        sb.Append(StunStatusEffectResistSecsExt);
        sb.Append(SlowStatusEffectResistSecsExt);
        sb.Append(FreezeStatusEffectResistSecsExt);

        return sb.ToString();
    }
}
