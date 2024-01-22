using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Players.Models.Character;
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
        timerThread.DelayCall(player.SetInvincibleFalse, player, TimeSpan.FromSeconds(1.5), TimeSpan.Zero, 1);
    }
    public static void ApplyDamageByPercent(this Player player, Room room, double percentage, TimerThread timerThread)
    {
        var health = (double)player.Character.Data.MaxLife;

        var damage = Convert.ToInt32(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, room, damage, timerThread);
    }

    public static void ApplyDamageByObject(this Player player, Room room, int objectId, TimerThread timerThread)
    {
        //temporary code until enemy/hazard system is implemented
        ApplyDamageByPercent(player, room, .10, timerThread);
    }

    public static void SetInvincibleTrue(this Player player, object invincibleData)
    {
        var playerData = (Player)invincibleData;
        if (!playerData.TempData.Invincible)
            playerData.TempData.Invincible = true;
    }
    public static void SetInvincibleFalse(this Player player, object invincibleData)
    {
        var playerData = (Player)invincibleData;
        if (playerData.TempData.Invincible)
            playerData.TempData.Invincible = false;
    }
}

