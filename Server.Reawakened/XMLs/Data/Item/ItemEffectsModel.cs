using A2m.Server;

public class ItemEffectsModel(List<ItemEffect> effects)
{
    public List<ItemEffect> Effects = effects;

    public ItemEffect GetItemEffect(ItemEffectType type)
    {
        foreach (var effect in Effects)
            if (type == effect.Type)
                return effect;
        return null;
    }
}
