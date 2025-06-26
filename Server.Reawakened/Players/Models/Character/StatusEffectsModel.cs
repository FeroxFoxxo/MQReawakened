using A2m.Server;
using Server.Reawakened.Database.Characters;

namespace Server.Reawakened.Players.Models.Character;

public class StatusEffectsModel(CharacterDbEntry entry)
{
    public Dictionary<ItemEffectType, StatusEffectModel> Effects => entry.StatusEffects;

    public void Add(ItemEffect effect)
    {
        var shouldReplaceEffect = true;

        if (Effects.TryGetValue(effect.Type, out var statusData))
            if (statusData.Value > effect.Value && statusData.Expiry > DateTime.Now)
                shouldReplaceEffect = false;

        if (shouldReplaceEffect)
        {
            var duration = TimeSpan.FromSeconds(effect.Duration);
            Remove(effect.Type);
            Effects.Add(effect.Type, new StatusEffectModel(effect.Type, effect.Value, DateTime.Now + duration));
        }
    }

    public void Remove(ItemEffectType effect) => Effects.Remove(effect);

    public float GetEffect(ItemEffectType effect)
    {
        var output = 0f;

        if (Effects.TryGetValue(effect, out var statusData))
            if (statusData.Effect == effect)
            {
                if (statusData.Expiry > DateTime.Now)
                    output = statusData.Value;
                else
                    Remove(effect);
            }

        return output;
    }

    public bool HasEffect(ItemEffectType effect)
    {
        if (Effects.TryGetValue(effect, out var statusData))
            if (statusData.Effect == effect)
            {
                if (statusData.Expiry > DateTime.Now)
                    return true;
                else
                    Remove(effect);
            }

        return false;
    }
}
