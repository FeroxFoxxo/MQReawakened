using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Base.Timers.Services;
using Server.Base.Timers.Extensions;
using A2m.Server;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public class InvincibiltyData()
    {
        public Player Player;
        public bool IsInvincible;
    }

    public static void SetTemporaryInvincibility(this Player player, TimerThread timerThread, double durationInSeconds)
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
        invinsibleData.Player.TempData.Invincible = invinsibleData.IsInvincible;
    }

    public static void ApplyCharacterDamage(this Player player, Room room, int damage)
    {
        if (player.TempData.Invincible) return;

        if (damage <= 0)
            damage = 1;

        player.Character.Data.CurrentLife -= damage;

        if (player.Character.Data.CurrentLife < 0)
            player.Character.Data.CurrentLife = 0;

        var health = new Health_SyncEvent(player.GameObjectId.ToString(), room.Time, player.Character.Data.CurrentLife, player.Character.Data.MaxLife, "Hurt");
        room.SendSyncEvent(health);
    }
    public static void ApplyDamageByPercent(this Player player, Room room, double percentage)
    {
        var health = (double)player.Character.Data.MaxLife;

        var damage = Convert.ToInt32(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, room, damage);
    }

    public static void ApplyDamageByObject(this Player player, Room room, int objectId) =>
        //temporary code until enemy/hazard system is implemented
        ApplyDamageByPercent(player, room, .10);
}
