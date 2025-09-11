using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;

public class AIProjectile : BaseProjectile
{
    private readonly string _ownerId;

    public AIProjectile(Room room, string ownerId, string projectileId, Vector3Model position, RectModel size,
        Vector2 speed, float lifeTime, TimerThread timerThread, int baseDamage,
        ItemEffectType effect, bool gravity, ServerRConfig config, ItemCatalog itemCatalog)
        : base(projectileId, lifeTime, room, position, speed, null, gravity)
    {
        _ownerId = ownerId;

        Collider = new AIProjectileCollider(
            projectileId, ownerId, room, Position,
            size, PrjPlane, LifeTime, timerThread,
            baseDamage, effect, itemCatalog, config
        );
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

        Room.Logger.LogTrace("AI Projectile {ProjectileId} hit entity {HitGoID} and was removed from the room.",
            ProjectileId, hitGoID);
    }
}
