using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Entity;
public class ChargeAttackEntity : TicklyEntity
{
    public TimerThread TimerThread;
    public ChargeAttackEntity(Player player, Vector3Model startPosition, Vector3Model endPosition, Vector2Model speed, float lifeTime, int itemId, int zoneId, int damage, Elemental type, TimerThread timerThread)
    {
        // Initialize charge attack location info
        Player = player;
        Position = startPosition;
        PrjPlane = Position.Z > 10 ? "Plane1" : "Plane0";

        // Initialize charge attack info
        SpawnPosition = new Vector3Model { X = Position.X, Y = Position.Y, Z = Position.Z };
        SpeedY = speed.Y;
        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;
        TimerThread = timerThread;

        // Send all information to room
        Collider = new AttackCollider(Player.GameObjectId, startPosition, 0.5f, 0.5f, PrjPlane, player, damage, type, 10f);
        Player.Room.SendSyncEvent(new ChargeAttackStart_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                        endPosition.X, endPosition.Y, speed.X, speed.Y, itemId, zoneId));
    }

    public override void Hit(string hitGoID)
    {
        //Logger.LogInformation("ChargeAttackStooped at position ({args1}, {args2}, {args3})", Position.X, Position.Y, Position.Z);
        Player.TempData.IsSuperStomping = false;
        Player.SetTemporaryInvincibility(TimerThread, 1.5);

        Player.Room.SendSyncEvent(new ChargeAttackStop_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
           Player.TempData.Position.X, Player.TempData.Position.Y, -1, -1, "1"));
        Player.Room.Projectiles.Remove(Player.GameObjectId);
    }
}
