using A2m.Server;

public class ItemEffectModel
{
    public float Duration;
    public string Type;
    public float Value;

    public ItemEffectModel(float duration, string type, float value)
    {
        Duration = duration;
        Type = type;
        Value = value;
    }
    public ItemEffectModel(ItemEffect effect)
    {
        Duration = effect.Duration;
        Type = effect.Type.ToString();
        Value = effect.Value;
    }
}
