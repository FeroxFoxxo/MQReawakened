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
        SpeedX = 0f;
        SpeedY = 0f;
        StartTime = player.Room.Time;
        LifeTime = StartTime + lifeTime;

        var meleeWidth = config.MeleeWidth;
        var meleeHeight = config.MeleeHeight;
        var isRight = direction > 0;

        _hitboxPosition = new Vector3Model { X = position.X, Y = position.Y, Z = position.Z };

        var onGround = Player.TempData.OnGround;

        if (onGround && !isRight)
        {
            _hitboxPosition.X -= config.MeleeXOffset;
            _hitboxPosition.Y += config.MeleeYOffset;
        }

        if (!onGround)
        {
            meleeWidth = config.MeleeArialWidth;
            meleeHeight = config.MeleeArialHeight;

            _hitboxPosition.X -= config.MeleeArialXOffset;
            _hitboxPosition.Y -= config.MeleeArialYOffset;
        }

        // Send all information to room
        Collider = new AttackCollider(id, _hitboxPosition, meleeWidth, meleeHeight, PrjPlane, player, damage, type, LifeTime, onGround ? 0.1f : 0.5f);
        var hitEvent = new Melee_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time, Position.X, Position.Y, Position.Z, direction, SpeedY, LifeTime, int.Parse(ProjectileID), item.PrefabName);
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
