using A2m.Server;

public class StatusEffectModel
{
    public ItemEffectType Effect { get; set; }
    public float Value { get; set; }
    public DateTime Expiry { get; set; }

    public StatusEffectModel(ItemEffectType effect, float value, DateTime expiry)
    {
        Effect = effect;
        Value = value;
        Expiry = expiry;
    }
}
