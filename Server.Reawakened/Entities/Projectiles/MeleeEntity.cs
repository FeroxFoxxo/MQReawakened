using A2m.Server;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;

public class MeleeEntity : BaseProjectile
{
    private readonly string _gameObjectId;

    public MeleeEntity(string id, Vector3 position, Player player, int direction, float lifeTime, ItemDescription item, int damage, Elemental type, ServerRConfig serverConfig, ItemRConfig config)
        : base(id, lifeTime, player.Room, position, new Vector2(0, 0), null, false, serverConfig)
    {
        _gameObjectId = player.GameObjectId;

        var onGround = player.TempData.OnGround;
        var isRight = direction > 0;

        var meleeLeft = onGround ? isRight ? 0 : -config.MeleeWidth : -config.MeleeAerialOffset;
        var meleeTop = onGround ? 0 : -config.MeleeAerialOffset;
        var meleeWidth = onGround ? config.MeleeWidth : config.MeleeAerialRange;
        var meleeHeight = onGround ? config.MeleeHeight : config.MeleeAerialRange;

        Collider = new AttackCollider(id, position, new Rect(meleeLeft, meleeTop, meleeWidth, meleeHeight),
            PrjPlane, player, damage, type, LifeTime, onGround ? 0.1f : 0.5f, player.Character.StatusEffects.HasEffect(ItemEffectType.Detect)
        );

        var hitEvent = new Melee_SyncEvent(
            _gameObjectId, Room.Time,
            Position.x, Position.y, Position.z, direction, Speed.y, LifeTime,
            int.Parse(ProjectileId), item.PrefabName
        );

        Room.SendSyncEvent(hitEvent);
    }

    public override void Hit(string hitGoID)
    {
        var hit = new MeleeHit_SyncEvent(new SyncEvent(_gameObjectId, SyncEvent.EventType.MeleeHit, Room.Time));

        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.x);
        hit.EventDataList.Add(Position.y);

        Room.SendSyncEvent(hit);
        Room.RemoveProjectile(ProjectileId);
    }
}
