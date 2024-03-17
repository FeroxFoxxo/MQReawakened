using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Base.Timers.Services;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{

    public static void ApplyCharacterDamage(this Player player, Room room, int damage, TimerThread timerThread)
    {
        if (player.TempData.Invincible) return;

        if (damage <= 0)
            damage = 1;

        player.Character.Data.CurrentLife -= damage;

        if (player.Character.Data.CurrentLife < 0)
            player.Character.Data.CurrentLife = 0;

        room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), room.Time,
            player.Character.Data.CurrentLife, player.Character.Data.MaxLife, "Hurt"));

        player.TemporaryInvincibility(timerThread, 1);
    }
    public static void ApplyDamageByPercent(this Player player, Room room, double percentage, TimerThread timerThread)
    {
        var health = (double)player.Character.Data.MaxLife;

        var damage = Convert.ToInt32(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, room, damage, timerThread);
    }

    // Temporary code until enemy/hazard system is implemented
    public static void ApplyDamageByObject(this Player player, Room room, string objectId, TimerThread timerThread) =>
        ApplyDamageByPercent(player, room, .10, timerThread);
}
