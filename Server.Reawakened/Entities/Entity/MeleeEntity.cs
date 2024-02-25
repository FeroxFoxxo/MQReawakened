using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Entity;

public class MeleeEntity : TicklyEntity
{
    private readonly Vector3Model _hitboxPosition;

    public MeleeEntity(Player player, string id, Vector3Model position, int direction, float lifeTime, ItemDescription item, int damage, Elemental type, ServerRConfig config)
    {
        // Initialize projectile location info
        Player = player;
        ProjectileID = id;
        Position = position;
        PrjPlane = Position.Z > 10 ? "Plane1" : "Plane0";

        // Initialize projectile info
        var isRight = direction > 0;
        Position.X += isRight ? 0 : 0;
        Position.Y += config.MeleeYOffset;
        SpawnPosition = new Vector3Model { X = Position.X, Y = Position.Y, Z = Position.Z };

        _hitboxPosition = new Vector3Model { X = Position.X, Y = Position.Y, Z = Position.Z };
        _hitboxPosition.X -= isRight ? 0 : config.MeleeWidth;
        SpeedX = 0;
        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;

        // Send all information to room
        Collider = new AttackCollider(id, _hitboxPosition, config.MeleeWidth, config.MeleeHeight, PrjPlane, player, damage, type, LifeTime);
        var hitEvent = new Melee_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time, Position.X, Position.Y, Position.Z, direction, 1, 1, int.Parse(ProjectileID), item.PrefabName);
        Player.Room.SendSyncEvent(hitEvent);
    }

    public override void Hit(string hitGoID)
    {
        //Logger.LogInformation("Projectile with ID {args1} destroyed at position ({args2}, {args3}, {args4})", ProjectileID, Position.X, Position.Y, Position.Z);
        var hit = new MeleeHit_SyncEvent(new SyncEvent(Player.GameObjectId, SyncEvent.EventType.MeleeHit, Player.Room.Time));
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        Player.Room.SendSyncEvent(hit);
        Player.Room.Projectiles.Remove(ProjectileID);
    }
}
