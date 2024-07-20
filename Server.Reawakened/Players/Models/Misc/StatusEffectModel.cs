using A2m.Server;

public class StatusEffectModel(ItemEffectModel status, bool active)
{
    public ItemEffectModel Status = status;
    public bool Active = active;

    public void SetActive(bool active) => Active = active;
}
