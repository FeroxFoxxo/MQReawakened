using System.Text;

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
        var sb = new StringBuilder();
        sb.Append(StandardDamageResistPointsInt);
        sb.Append(DataDelimiter);
        sb.Append(FireDamageResistPointsInt);
        sb.Append(DataDelimiter);
        sb.Append(IceDamageResistPointsInt);
        sb.Append(DataDelimiter);
        sb.Append(PoisonDamageResistPointsInt);
        sb.Append(DataDelimiter);
        sb.Append(LightningDamageResistPointsInt);
        sb.Append(DataDelimiter);
        sb.Append(StandardDamageResistPointsExt);
        sb.Append(DataDelimiter);
        sb.Append(FireDamageResistPointsExt);
        sb.Append(DataDelimiter);
        sb.Append(IceDamageResistPointsExt);
        sb.Append(DataDelimiter);
        sb.Append(PoisonDamageResistPointsExt);
        sb.Append(DataDelimiter);
        sb.Append(LightningDamageResistPointsExt);
        sb.Append(DataDelimiter);
        sb.Append(StunStatusEffectResistSecsExt);
        sb.Append(DataDelimiter);
        sb.Append(SlowStatusEffectResistSecsExt);
        sb.Append(DataDelimiter);
        sb.Append(FreezeStatusEffectResistSecsExt);

        return sb.ToString();
    }
}
