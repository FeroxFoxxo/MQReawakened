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
    public static void ApplySlowEffect(this Player player, string hazardId, int damage) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.SlowStatusEffect, damage, 1, true, hazardId, false));

    //Doesn't seem to apply fast enough.
    public static void NullifySlowStatusEffect(this Player player, string hazardId) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.NullifySlowStatusEffect, 1, 1, true, hazardId, true));

    public static bool HasNullifyEffect(this Player player, ItemCatalog itemCatalog) =>
     player.Character.Equipment.EquippedItems
         .Select(x => itemCatalog.GetItemFromId(x.Value))
         .Any(item => item.ItemEffects.Any(effect => effect.Type == ItemEffectType.NullifySlowStatusEffect));

    public static void StartPoisonDamage(this Player player, string hazardId, int damage, int hurtLength, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        if (player == null || player.TempData.Invincible)
            return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.PoisonDamage, damage, hurtLength, true, hazardId, false));

        player.ApplyCharacterDamage(damage, hazardId, hurtLength, serverRConfig, timerThread);
    }

    public static void TemporaryInvincibility(this Player player, TimerThread timerThread,
        ServerRConfig serverRConfig, double durationInSeconds)
    {
        player.TempData.Invincible = true;

        //ItemEffectType Invincibility doesn't exist <= Late 2012.
        var effectType = serverRConfig.GameVersion > Core.Enums.GameVersion.vLate2012 ? ItemEffectType.Invincibility : ItemEffectType.Unknown;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                 (int)effectType, 0, (int)durationInSeconds, true, player.CharacterName, true));

        var invincibleData = new InvincibilityData()
        {
            Player = player,
            IsInvincible = false
        };

        timerThread.RunDelayed(DisableInvincibility, invincibleData, TimeSpan.FromSeconds(durationInSeconds));
    }

    public class InvincibilityData() : PlayerTimer
    {
        public bool IsInvincible;
    }

    public static void DisableInvincibility(ITimerData data)
    {
        if (data is not InvincibilityData invincible)
            return;

        invincible.Player.TempData.Invincible = false;
    }
}
