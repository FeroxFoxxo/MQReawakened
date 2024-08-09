using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public class UnderwaterData() : PlayerRoomTimer
    {
        public int Damage;
        public TimerThread TimerThread;
        public ServerRConfig ServerRConfig;
    }

    public static void StartUnderwater(this Player player, int damage, TimerThread timerThread, ServerRConfig serverRConfig)
    {
        player.StopUnderwater();

        var underwaterData = new UnderwaterData()
        {
            Damage = damage,
            TimerThread = timerThread,
            ServerRConfig = serverRConfig,
            Player = player
        };

        var ticksTillDeath = (int)Math.Ceiling((double)player.Character.CurrentLife / damage);

        player.TempData.Underwater = true;
        player.TempData.UnderwaterTimer = timerThread.RunInterval(ApplyUnderwaterDamage, underwaterData,
            TimeSpan.FromSeconds(serverRConfig.UnderwaterDamageInterval), ticksTillDeath, TimeSpan.FromSeconds(serverRConfig.BreathTimerDuration));
    }

    public static void ApplyUnderwaterDamage(ITimerData data)
    {
        if (data is not UnderwaterData water)
            return;

        if (!water.Player.TempData.Underwater)
            return;

        water.Player.Room.SendSyncEvent(new StatusEffect_SyncEvent(water.Player.GameObjectId, water.Player.Room.Time,
            (int)ItemEffectType.WaterDamage, water.Damage, 1, true, water.Player.GameObjectId, false));

        water.Player.ApplyCharacterDamage(water.Player.Character.MaxLife / water.ServerRConfig.UnderwaterDamageRatio,
            string.Empty, 1, water.ServerRConfig, water.TimerThread);
    }

    public static void StopUnderwater(this Player player)
    {
        if (player.TempData.UnderwaterTimer != null)
        {
            player.TempData.UnderwaterTimer.Stop();
            player.TempData.UnderwaterTimer = null;
        }
    }

    public static void ApplyCharacterDamage(this Player player, float damage, string originId, double invincibilityDuration, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        if (player.TempData.Invincible) return;

        if (damage <= 0)
            damage = 1;

        var isShielded = false;

        if (player.Character.Pets.TryGetValue(player.GetEquippedPetId(serverRConfig), out var pet))
        {
            if (player.TempData.PetDefense)
            {
                isShielded = true;
                Math.Ceiling(damage *= pet.AbilityParams.DefensiveBonusRatio);
            }
        }

        if (!isShielded)
            player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.BluntDamage, (int)damage, (int)invincibilityDuration, true, originId, false));

        player.Character.Write.CurrentLife -= (int)damage;

        if (player.Character.CurrentLife < 0)
            player.Character.Write.CurrentLife = 0;

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.Character.CurrentLife, player.Character.MaxLife, originId));

        if (invincibilityDuration <= 0)
            invincibilityDuration = 1;

        player.TemporaryInvincibility(timerThread, serverRConfig, invincibilityDuration);
    }

    public static void ApplyDamageByPercent(this Player player, double percentage, string hazardId, float duration, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        var health = (double)player.Character.MaxLife;

        var damage = Convert.ToSingle(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, damage, hazardId, duration, serverRConfig, timerThread);
    }
}
