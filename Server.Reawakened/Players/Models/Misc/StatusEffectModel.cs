using A2m.Server;

public class StatusEffectModel(ItemEffectModel status, DateTime expiry)
{
    public ItemEffectModel Status = status;
    public readonly DateTime Expiry = expiry;
}
