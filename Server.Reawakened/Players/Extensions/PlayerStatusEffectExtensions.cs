using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerStatusEffectExtensions
{
    public class StatusEffectData() : PlayerTimer
    {
        public string HazardId;
        public int HazardDamage;
        public ItemEffectType EffectType;
        public float InitialDamageDelay;
        public float DamageDelay;
        public int DamageCount;
        public int InvincibilityDuration;
        public TimerThread TimerThread;
    }

    public static void SendItemEffectToPlayer(this Player player, ItemEffect itemEffect, string prefabFrom, bool sendFx, bool premium = false)
    {
        if (player == null || itemEffect == null) return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time, (int)itemEffect.Type,
            itemEffect.Value, itemEffect.Duration, sendFx, prefabFrom, premium));
    }

    // DAMAGE
    public static void StartDamageEffect(this Player player, ItemEffectType effectType,
        int effectValue, int effectDuration, string originId)
    {
        if (player == null) return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)effectType, effectValue, effectDuration, true, originId, false));
    }

    public static void StopDamageEffect(this Player player, ITimerData data)
    {
        if (data is not StatusEffectData status) return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)status.EffectType, -1, -1, false, status.HazardId, false));
    }

    // SLOW EFFECT
    public static void ApplySlowEffect(this Player player)
    {
        player.TempData.IsSlowed = true;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.SlowStatusEffect, -1, -1, true, string.Empty, false));
    }

    public static void NullifySlowStatusEffect(this Player player)
    {
        if (player == null) return;

        player.TempData.IsSlowed = false;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.SlowStatusEffect, -1, -1, false, string.Empty, false));

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.NullifySlowStatusEffect, 1, 1, true, string.Empty, false));
    }

    public static void RunUnderwaterTick(this Player player, ServerRConfig config)
    {
        player.TempData.Underwater = true;
        
        if (player.Character.StatusEffects.GetEffect(ItemEffectType.WaterBreathing) > 0)
        {
            player.ResetUnderwaterTime();
            return;
        }

        var currentTime = player.Room.Time;

        if (player.TempData.UnderwaterTime == 0)
        {
            player.TempData.UnderwaterTime = currentTime;
            return;
        }

        if (currentTime - player.TempData.UnderwaterTime < config.BaseUnderwaterTime)
            return;

        player.ApplyDamageByPercent(0.1, ItemEffectType.WaterDamage, "0", 1);
        player.TempData.UnderwaterTime += 2.5f;
    }

    public static void ResetUnderwaterTime(this Player player) => player.TempData.UnderwaterTime = 0;

    public static bool HasNullifyEffect(this Player player, ItemCatalog itemCatalog) =>
     player.Character.Equipment.EquippedItems
         .Select(x => itemCatalog.GetItemFromId(x.Value))
         .Any(item => item != null && item.ItemEffects.Any(effect => effect.Type == ItemEffectType.NullifySlowStatusEffect));


    public static void ApplyPoisonDamage(ITimerData data)
    {
        if (data == null || data is not StatusEffectData poisonData) return;

        var player = poisonData.Player;

        player.TempData.IsPoisoned = true;

        poisonData.TimerThread.RunInterval(player.ApplyPoisonDamage, poisonData,
            TimeSpan.FromSeconds(poisonData.DamageDelay), poisonData.DamageCount, TimeSpan.FromSeconds(poisonData.InitialDamageDelay));
    }

    public static void StopPoisonEffect(this Player player)
    {
        player.TempData.IsPoisoned = false;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.PoisonDamage, -1, -1, false, string.Empty, false));

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.CurePoison, 1, 1, true, string.Empty, false));
    }

    public static void TemporaryInvincibility(this Player player, double durationInSeconds)
    {
        if (durationInSeconds <= 0 || player?.Room == null) return;

        var added = player.Character.StatusEffects.TryAddIfInactive(
            new StatusEffectModel(ItemEffectType.Invincibility, 1,
                DateTime.UtcNow + TimeSpan.FromSeconds(durationInSeconds), string.Empty));

        if (!added) return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.Invincibility, 1, (int)durationInSeconds, true, string.Empty, false));
    }

    public static StatusEffectData GetPoisonEffectData(this Player player, int poisonDamage, string hazardId,
        HazardRConfig hazardRConfig, TimerThread timerThread, int initDelay, int delay)
    {
        var damageCount = (int)Math.Ceiling((double)player.Character.CurrentLife / poisonDamage);
        var invincibilityDuration = -1;

        var hazardIsEnemy = player.Room.ContainsEnemy(hazardId);

        if (hazardIsEnemy)
        {
            poisonDamage /= hazardRConfig.PoisonDamageOverTimeDeduction;
            damageCount = hazardRConfig.PoisonDamageCountFromEnemy;
            initDelay = 0;
            delay = hazardRConfig.PoisonEffectInterval;
            invincibilityDuration = 1;
        }

        return new StatusEffectData()
        {
            Player = player,
            HazardDamage = poisonDamage,
            EffectType = ItemEffectType.PoisonDamage,
            HazardId = hazardId,
            InitialDamageDelay = initDelay,
            DamageDelay = delay,
            DamageCount = damageCount,
            InvincibilityDuration = invincibilityDuration,
            TimerThread = timerThread
        };
    }
}
