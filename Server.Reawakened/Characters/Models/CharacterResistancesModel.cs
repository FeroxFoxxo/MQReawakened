using System.Text;

namespace Server.Reawakened.Characters.Models;

public class CharacterResistancesModel
{
    public const char DataDelimiter = '|';

    public int StandardDamageResistPointsInt;
    public int FireDamageResistPointsInt;
    public int IceDamageResistPointsInt;
    public int PoisonDamageResistPointsInt;
    public int LightningDamageResistPointsInt;
    public int StandardDamageResistPointsExt;
    public int FireDamageResistPointsExt;
    public int IceDamageResistPointsExt;
    public int PoisonDamageResistPointsExt;
    public int LightningDamageResistPointsExt;
    public int StunStatusEffectResistSecsExt;
    public int SlowStatusEffectResistSecsExt;
    public int FreezeStatusEffectResistSecsExt;

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
