using A2m.Server;
using Server.Reawakened.Database.Characters;

namespace Server.Reawakened.Players.Models.Character;

public class StatusEffectsModel(CharacterDbEntry entry)
{
    private readonly object _lock = new();

    // Raw access is unsafe. Marked obsolete so every remaining caller shows up as a warning.
    [Obsolete("Not thread-safe. Use the methods on StatusEffectsModel or Snapshot().")]
    public Dictionary<ItemEffectType, StatusEffectModel> Effects => entry.StatusEffects;

    public void Add(ItemEffect effect, string prefabName)
    {
        lock (_lock)
        {
            var effects = entry.StatusEffects;

            if (effects.TryGetValue(effect.Type, out var existing) &&
                existing.Value > effect.Value && existing.Expiry > DateTime.UtcNow)
                return;

            var duration = TimeSpan.FromSeconds(effect.Duration);
            effects[effect.Type] = new StatusEffectModel(
                effect.Type, effect.Value, DateTime.UtcNow + duration, prefabName);
        }
    }

    public bool TryAddIfInactive(StatusEffectModel model)
    {
        lock (_lock)
        {
            var effects = entry.StatusEffects;

            if (effects.TryGetValue(model.Effect, out var existing) && existing.Expiry > DateTime.UtcNow)
                return false;

            effects[model.Effect] = model;
            return true;
        }
    }

    public void Remove(ItemEffectType effect)
    {
        lock (_lock)
            entry.StatusEffects.Remove(effect);
    }

    public void UpdateStatus()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;

            var expired = entry.StatusEffects
                .Where(x => x.Value == null || x.Value.Expiry <= now)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expired)
                entry.StatusEffects.Remove(key);
        }
    }

    public float GetEffect(ItemEffectType effect)
    {
        lock (_lock)
        {
            if (!entry.StatusEffects.TryGetValue(effect, out var data) || data.Effect != effect)
                return 0f;

            if (data.Expiry > DateTime.UtcNow)
                return data.Value;

            entry.StatusEffects.Remove(effect);
            return 0f;
        }
    }

    public bool HasEffect(ItemEffectType effect)
    {
        lock (_lock)
        {
            if (!entry.StatusEffects.TryGetValue(effect, out var data) || data.Effect != effect)
                return false;

            if (data.Expiry > DateTime.UtcNow)
                return true;

            entry.StatusEffects.Remove(effect);
            return false;
        }
    }

    public Dictionary<ItemEffectType, StatusEffectModel> Snapshot()
    {
        lock (_lock)
            return new Dictionary<ItemEffectType, StatusEffectModel>(entry.StatusEffects);
    }
}
