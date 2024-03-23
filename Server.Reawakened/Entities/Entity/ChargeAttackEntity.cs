using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Entity;
public class ChargeAttackEntity : TicklyEntity
{
    public TimerThread TimerThread;

    public int ItemId;
    public int ZoneId;

    public ChargeAttackEntity(Player player, Vector3Model startPosition, Vector3Model endPosition, Vector2Model speed, float lifeTime, int itemId, int zoneId, int damage, Elemental type, TimerThread timerThread)
    {
        // Initialize charge attack location info
        Player = player;
        Position = startPosition;
        PrjPlane = Position.Z > 10 ? "Plane1" : "Plane0";

        // Initialize charge attack info
        SpawnPosition = new Vector3Model { X = Position.X, Y = Position.Y, Z = Position.Z };
        ChargeEndPosition = endPosition;
        SpeedY = speed.Y;
        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;
        TimerThread = timerThread;

        ZoneId = zoneId;
        ItemId = itemId;

        // Send all information to room
        Collider = new AttackCollider(Player.GameObjectId, startPosition, 1, 1, PrjPlane, player, damage, type, 15f, 0);
        Player.Room.SendSyncEvent(new ChargeAttackStart_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                        endPosition.X, endPosition.Y, speed.X, speed.Y, itemId, zoneId));
    }

    public override void Hit(string hitGoID)
    {
        //Logger.LogInformation("ChargeAttackStooped at position ({args1}, {args2}, {args3})", Position.X, Position.Y, Position.Z);
        Player.TempData.IsSuperStomping = false;
        Player.TemporaryInvincibility(TimerThread, 1);

        Player.Room.SendSyncEvent(new ChargeAttackStop_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
           Player.TempData.Position.X, Player.TempData.Position.Y, ItemId, ZoneId, hitGoID));

        Player.Room.Projectiles.Remove(Player.GameObjectId);
    }
}
