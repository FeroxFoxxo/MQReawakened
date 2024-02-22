using A2m.Server;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Base.Timers.Services;
using Server.Base.Timers.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public class InvincibiltyData()
    {
        public Player Player;
        public bool IsInvincible;
    }

    public static void SetTempInvincibility(this Player player, TimerThread timerThread, double durationInSeconds)
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

    public static AttackCollider ChargeAttackCollider(this Player player)
       => new(player.GameObjectId.ToString(), new Vector3Model()
       {
           X = player.TempData.Position.X,
           Y = player.TempData.Position.Y - 4f,
           Z = player.TempData.Position.Z
       },
       4f, 2f, player.GetPlayersPlaneString(player.DatabaseContainer.ServerRConfig),
       player, 25, Elemental.Standard, 10f);

    public static void SendStuperStompCollision(this Player player, string entityId)
    {
        if (!player.Room.Colliders.ContainsKey(entityId)) return;

        var destructibleCollider = player.Room.Colliders[entityId];
        destructibleCollider.SendCollisionEvent(player.ChargeAttackCollider());

        player.Room.SendSyncEvent(new ChargeAttackStop_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            destructibleCollider.Position.x, destructibleCollider.Position.y, -1, -1, "1"));

        return;
    }

    public static void ApplyCharacterDamage(this Player player, Room room, int damage)
    {
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
