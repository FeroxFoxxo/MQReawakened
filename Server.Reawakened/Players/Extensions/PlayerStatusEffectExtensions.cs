using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
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
     player.Character.Data.Equipment.EquippedItems
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

    public class InvisibilityData()
    {
        public Player Player;
        public bool IsInvisible;
        public float Duration;
    }

    public static void TemporaryInvisibility(this Player player, float duration, TimerThread timerThread)
    {
        player.TempData.Invisible = true;

        var disableInvisibilityData = new InvisibilityData()
        {
            Player = player,
            IsInvisible = false,
            Duration = duration
        };

        timerThread.DelayCall(DisableInvisibility, disableInvisibilityData,
            TimeSpan.FromSeconds(duration), TimeSpan.Zero, 1);
    }

    public static void DisableInvisibility(object data)
    {
        var invisibilityData = (InvisibilityData)data;
        invisibilityData.Player.TempData.Invisible = false;
    }

    public class InvincibilityData()
    {
        public Player Player;
        public bool IsInvincible;
    }

    public static void TemporaryInvincibility(this Player player, TimerThread timerThread, double durationInSeconds)
    {
        player.TempData.Invincible = true;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                 (int)ItemEffectType.Invincibility, 0, (int)durationInSeconds, true, player.CharacterName, true));

        var invincibleData = new InvincibilityData()
        {
            Player = player,
            IsInvincible = false
        };

        timerThread.DelayCall(DisableInvincibility, invincibleData, TimeSpan.FromSeconds(durationInSeconds), TimeSpan.Zero, 1);
    }

    public static void DisableInvincibility(object data)
    {
        var invincibleData = (InvincibilityData)data;
        invincibleData.Player.TempData.Invincible = false;
    }
}
