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
    private readonly Vector3 _hitboxPosition;
    private readonly string _gameObjectId;

    public MeleeEntity(string id, Vector3 position, Player player, int direction, float lifeTime, ItemDescription item, int damage, Elemental type, ServerRConfig serverConfig, ItemRConfig config)
        : base(id, 0, 0, lifeTime, player.Room, position, null, false, serverConfig)
    {
        _gameObjectId = player.GameObjectId;

        var onGround = player.TempData.OnGround;
        var isRight = direction > 0;

        var meleeWidth = onGround ? config.MeleeWidth : config.MeleeArialWidth;
        var meleeHeight = onGround ? config.MeleeHeight : config.MeleeArialHeight;

        _hitboxPosition = new Vector3 { x = position.x, y = position.y, z = position.z };

        _hitboxPosition.x -= isRight ? 0 : meleeWidth / 2;
        _hitboxPosition.y -= meleeHeight / 2;

        Collider = new AttackCollider(id, _hitboxPosition, new Vector2(meleeWidth, meleeHeight),
            PrjPlane, player, damage, type, LifeTime, onGround ? 0.1f : 0.5f
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
