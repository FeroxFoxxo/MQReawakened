using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public class UnderwaterData()
    {
        public Player Player;
        public int Damage;
        public TimerThread TimerThread;
    }

    public static void StartUnderwater(this Player player, int damage, TimerThread timerThread, ItemRConfig config, Microsoft.Extensions.Logging.ILogger logger)
    {
        player.StopUnderwater(logger);

        logger.LogDebug("Player '{CharacterName}' has gotten into the water!", player.CharacterName);

        var underwaterData = new UnderwaterData()
        {
            Player = player,
            Damage = damage,
            TimerThread = timerThread
        };

        var ticksTillDeath = (int)Math.Ceiling((double)player.Character.Data.CurrentLife / damage);

        player.TempData.Underwater = true;
        player.TempData.UnderwaterTimer = timerThread.DelayCall(ApplyUnderwaterDamage, underwaterData,
            TimeSpan.FromSeconds(config.BreathTimerDuration), TimeSpan.FromSeconds(config.UnderwaterDamageInterval), ticksTillDeath);
    }

    public static void StopUnderwater(this Player player, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (player.TempData.Underwater)
        {
            logger.LogDebug("Player '{CharacterName}' has gotten out of the water!", player.CharacterName);
            player.TempData.Underwater = false;
        }

        if (player.TempData.UnderwaterTimer != null)
        {
            player.TempData.UnderwaterTimer.Stop();
            player.TempData.UnderwaterTimer = null;
        }
    }

    public static void ApplyUnderwaterDamage(object underwaterData)
    {
        var waterData = (UnderwaterData)underwaterData;

        if (!waterData.Player.TempData.Underwater)
            return;

        waterData.Player.Room.SendSyncEvent(new StatusEffect_SyncEvent(waterData.Player.GameObjectId, waterData.Player.Room.Time,
            (int)ItemEffectType.WaterDamage, waterData.Damage, 1, true, waterData.Player.GameObjectId, false));

        waterData.Player.ApplyCharacterDamage(waterData.Player.Character.Data.MaxLife / 10, 1, waterData.TimerThread);
    }

    public static void ApplyCharacterDamage(this Player player, int damage, double invincibilityDuration, TimerThread timerThread)
    {
        if (player.TempData.Invincible) return;

        if (damage <= 0)
            damage = 1;

        player.Character.Data.CurrentLife -= damage;

        if (player.Character.Data.CurrentLife < 0)
            player.Character.Data.CurrentLife = 0;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.Character.Data.CurrentLife, player.Character.Data.MaxLife, "Hurt"));

        if (invincibilityDuration <= 0)
            invincibilityDuration = 1;

        player.TemporaryInvincibility(timerThread, invincibilityDuration);
    }

    public static void ApplyDamageByPercent(this Player player, double percentage, string hazardId, float duration, TimerThread timerThread)
    {
        var health = (double)player.Character.Data.MaxLife;

        var damage = Convert.ToInt32(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, damage, duration, timerThread);
    }
}
