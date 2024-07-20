using A2m.Server;
using Server.Reawakened.Database.Characters;

public class StatusEffectModel
{
    public ItemEffectType Effect;
    public float Value;
    public DateTime Expiry;

    public StatusEffectModel()
    {
    }

    public StatusEffectModel(ItemEffectType effect, float value, DateTime expiry)
    {
        Effect = effect;
        Value = value;
        Expiry = expiry;
    }
}
