using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public static void ApplyCharacterDamage(this Player player, Room room, int damage, TimerThread timerThread)
    {
        player.Character.Data.CurrentLife -= damage;

        if (player.Character.Data.CurrentLife < 0)
            player.Character.Data.CurrentLife = 0;

        room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), room.Time,
            player.Character.Data.CurrentLife, player.Character.Data.MaxLife, "Hurt"));

        player.TempData.Invincible = true;
        timerThread.DelayCall(player.DisableInvincibility, player, TimeSpan.FromSeconds(1.5), TimeSpan.Zero, 1);
    }
    public static void ApplyDamageByPercent(this Player player, Room room, double percentage, TimerThread timerThread)
    {
        var health = (double)player.Character.Data.MaxLife;

        var damage = Convert.ToInt32(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, room, damage, timerThread);
    }

    public static void ApplyDamageByObject(this Player player, Room room, int _, TimerThread timerThread) =>
        //temporary code until enemy/hazard system is implemented
        ApplyDamageByPercent(player, room, .10, timerThread);

    public static void DisableInvincibility(this Player player, object invincibleData)
    {
        var playerData = (Player)invincibleData;
        playerData.TempData.Invincible = false;
    }
}

