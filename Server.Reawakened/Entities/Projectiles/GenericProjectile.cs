using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Projectiles;
public class GenericProjectile : BaseProjectile
{
    private readonly Vector3Model _hitboxPosition;
    private readonly string _gameObjectId;
    private readonly float _gravityFactor;

    public override BaseCollider Collider { get; set; }

    public GenericProjectile(string id, Player player, float lifeTime, Vector3Model position, ItemRConfig config, ServerRConfig serverConfig,
        int direction, ItemDescription item, int damage, Elemental damageType, bool isGrenade)
            : base(id, config.ProjectileSpeedX * (direction > 0 ? 1 : -1), isGrenade ? config.GrenadeSpeedY : config.ProjectileSpeedY, lifeTime, player.Room,
                new Vector3Model(position.X + config.ProjectileXOffset * (direction > 0 ? 1 : -1), position.Y + config.ProjectileYOffset, position.Z), null, serverConfig)
    {
        _gameObjectId = player.GameObjectId;
        _gravityFactor = isGrenade ? config.GrenadeGravityFactor : config.ProjectileGravityFactor;

        _hitboxPosition = new Vector3Model { X = Position.X, Y = Position.Y - config.ProjectileHeight, Z = Position.Z };
        _hitboxPosition.X -= direction > 0 ? 0 : config.ProjectileWidth;

        Collider = new AttackCollider(id, _hitboxPosition, config.ProjectileWidth, config.ProjectileHeight, PrjPlane, player, damage, damageType, LifeTime, 0);

        var prj = new LaunchItem_SyncEvent(_gameObjectId, StartTime, Position.X, Position.Y, Position.Z, SpeedX, SpeedY, LifeTime, int.Parse(ProjectileId), item.PrefabName);
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
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        Room.SendSyncEvent(hit);
        Room.RemoveProjectile(ProjectileId);
    }
}
