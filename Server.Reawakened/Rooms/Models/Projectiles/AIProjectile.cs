using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities.Projectiles;
public class AIProjectile : BaseProjectile
{
    private readonly string _ownerId;

    public override BaseCollider Collider { get; set; }

    public AIProjectile(Room room, string ownerId, string projectileId, Vector3Model position,
        float speedX, float speedY, float lifeTime, TimerThread timerThread, int baseDamage, ItemEffectType effect, ServerRConfig config, ItemCatalog itemCatalog)
        : base(projectileId, speedX, speedY, lifeTime, room, position, null, config)
    {
        _ownerId = ownerId;
        Collider = new AIProjectileCollider(projectileId, ownerId, room, projectileId, position, 0.5f, 0.5f, PrjPlane, LifeTime, timerThread, baseDamage, effect, itemCatalog);
    }

    public override void Move()
    {
        Position.X = SpawnPosition.X + (Room.Time - StartTime) * SpeedX;
        Collider.Position.x = Position.X;

        Position.Y = SpawnPosition.Y + (Room.Time - StartTime) * SpeedY;
        Collider.Position.y = Position.Y;
    }

    public override void Hit(string hitGoID)
    {
        var hit = new ProjectileHit_SyncEvent(new SyncEvent(_ownerId, SyncEvent.EventType.ProjectileHit, Room.Time));

        hit.EventDataList.Add(int.Parse(ProjectileId));
        hit.EventDataList.Add(hitGoID);
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        Room.SendSyncEvent(hit);
        Room.RemoveProjectile(ProjectileId);
    }
}
