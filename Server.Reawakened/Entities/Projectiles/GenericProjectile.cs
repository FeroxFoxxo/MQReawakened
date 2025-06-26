using A2m.Server;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;
public class GenericProjectile : BaseProjectile
{
    private readonly Vector3 _hitboxPosition;
    private readonly string _gameObjectId;
    private readonly float _gravityFactor;

    public GenericProjectile(string id, Player player, float lifeTime, Vector3 position, ItemRConfig config, ServerRConfig serverConfig,
        int direction, ItemDescription item, int damage, Elemental damageType, bool isGrenade)
            : base(id, lifeTime, player.Room,
                new Vector3(position.x + config.ProjectileXOffset * (direction > 0 ? 1 : -1), position.y + config.ProjectileYOffset, position.z),
                new Vector2(config.ProjectileSpeedX * (direction > 0 ? 1 : -1), isGrenade ? config.GrenadeSpeedY : config.ProjectileSpeedY),
                null, false, serverConfig)
    {
        _gameObjectId = player.GameObjectId;
        _gravityFactor = isGrenade ? config.GrenadeGravityFactor : config.ProjectileGravityFactor;

        _hitboxPosition = new Vector3 { x = Position.x, y = Position.y - config.ProjectileHeight, z = Position.z };
        _hitboxPosition.x -= direction > 0 ? 0 : config.ProjectileWidth;

        Collider = new AttackCollider(id, _hitboxPosition, new Rect(0, 0, config.ProjectileWidth, config.ProjectileHeight), PrjPlane, player, damage, damageType, LifeTime, 0, player.Character.StatusEffects.HasEffect(ItemEffectType.Detect));

        var prj = new LaunchItem_SyncEvent(_gameObjectId, StartTime, Position.x, Position.y, Position.z, Speed.x, Speed.y, LifeTime, int.Parse(ProjectileId), item.PrefabName);
        Room.SendSyncEvent(prj);
    }

    public override void Move()
    {
        DecreaseSpeedY(_gravityFactor);
        base.Move();
    }

    public override void Hit(string hitGoID)
    {
        var hit = new ProjectileHit_SyncEvent(new SyncEvent(_gameObjectId, SyncEvent.EventType.ProjectileHit, Room.Time));

        hit.EventDataList.Add(int.Parse(ProjectileId));
        hit.EventDataList.Add(hitGoID);
        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.x);
        hit.EventDataList.Add(Position.y);

        Room.SendSyncEvent(hit);
        Room.RemoveProjectile(ProjectileId);
    }
}
