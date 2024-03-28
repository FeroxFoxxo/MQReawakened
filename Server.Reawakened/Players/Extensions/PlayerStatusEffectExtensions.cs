using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Extensions;


namespace Server.Reawakened.Players.Extensions;

public static class PlayerStatusEffectExtensions
{
    public static void ApplySlowEffect(this Player player, string hazardId, int damage) => 
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.SlowStatusEffect, damage, 1, true, hazardId, false));

    //Doesn't seem to apply fast enough.
    public static void NullifySlowStatusEffect(this Player player, string hazardId) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.NullifySlowStatusEffect, 1, 1, true, hazardId, false));

    public static void StartPoisonDamage(this Player player, string hazardId, int damage, int hurtLength, TimerThread timerThread)
    {
        if (player == null || player.TempData.Invincible)
            return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.PoisonDamage, damage, hurtLength, true, hazardId, false));

        player.ApplyCharacterDamage(damage, hurtLength, timerThread);
    }

    public class InvisibiltyData()
    {
        public Player Player;
        public bool IsInvisibile;
        public float Duration;
    }

    public static void TemporaryInvisibility(this Player player, float duration, TimerThread timerThread)
    {
        player.TempData.Invisible = true;

        var disableInvisibilityData = new InvisibiltyData()
        {
            Player = player,
            IsInvisibile = false,
            Duration = duration
        };

        timerThread.DelayCall(DisableInvisibility, disableInvisibilityData,
            TimeSpan.FromSeconds(duration), TimeSpan.Zero, 1);
    }

    public static void DisableInvisibility(object data)
    {
        var invisibilityData = (InvisibiltyData)data;

        invisibilityData.Player.TempData.Invisible = false;
    }

    public class InvincibiltyData()
    {
        public Player Player;
        public bool IsInvincible;
    }

    public static void TemporaryInvincibility(this Player player, TimerThread timerThread, double durationInSeconds)
    {
        player.TempData.Invincible = true;

        var invinsibleData = new InvincibiltyData()
        {
            Player = player,
            IsInvincible = false
        };

        timerThread.DelayCall(DisableInvincibility, invinsibleData, TimeSpan.FromSeconds(durationInSeconds), TimeSpan.Zero, 1);
    }

    public static void DisableInvincibility(object data)
    {
        var invinsibleData = (InvincibiltyData)data;
        invinsibleData.Player.TempData.Invincible = false;
    }
}
