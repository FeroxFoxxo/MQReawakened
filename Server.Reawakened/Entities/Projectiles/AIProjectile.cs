using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;
public class AIProjectile : BaseProjectile
{
    private readonly string _ownerId;

    public AIProjectile(Room room, string ownerId, string projectileId, Vector3 position,
        Vector2 speed, float lifeTime, TimerThread timerThread, int baseDamage,
        ItemEffectType effect, bool gravity, ServerRConfig config, ItemCatalog itemCatalog, ItemRConfig itemConfig)
        : base(projectileId, lifeTime, room, position, speed, null, gravity, config)
    {
        _ownerId = ownerId;

        Collider = new AIProjectileCollider(
            projectileId, ownerId, room, projectileId, position,
            new Rect(0.5f, 0.5f, 0.5f, 0.5f), PrjPlane, LifeTime, timerThread,
            baseDamage, effect, itemCatalog, itemConfig, config
        );
    }

    public override void Hit(string hitGoID)
    {
        var hit = new ProjectileHit_SyncEvent(new SyncEvent(_ownerId, SyncEvent.EventType.ProjectileHit, Room.Time));

        hit.EventDataList.Add(int.Parse(ProjectileId));
        hit.EventDataList.Add(hitGoID);
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.x);
        hit.EventDataList.Add(Position.y);

        Room.SendSyncEvent(hit);
        Room.RemoveProjectile(ProjectileId);
    }
}
