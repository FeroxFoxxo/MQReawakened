using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Projectiles;

public class MeleeEntity : BaseProjectile
{
    private readonly Vector3Model _hitboxPosition;
    private readonly string _gameObjectId;

    public override BaseCollider Collider { get; set; }

    public MeleeEntity(string id, Vector3Model position, Player player, int direction, float lifeTime, ItemDescription item, int damage, Elemental type, ServerRConfig serverConfig, ItemRConfig config)
        : base(id, 0, 0, lifeTime, player.Room, position, null, serverConfig)
    {
        _gameObjectId = player.GameObjectId;

        var meleeWidth = config.MeleeWidth;
        var meleeHeight = config.MeleeHeight;
        var isRight = direction > 0;

        _hitboxPosition = new Vector3Model { X = position.X, Y = position.Y, Z = position.Z };

        var onGround = player.TempData.OnGround;

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

        Collider = new AttackCollider(id, _hitboxPosition, meleeWidth, meleeHeight, PrjPlane, player, damage, type, LifeTime, onGround ? 0.1f : 0.5f);

        var hitEvent = new Melee_SyncEvent(_gameObjectId, Room.Time, Position.X, Position.Y, Position.Z, direction, SpeedY, LifeTime, int.Parse(ProjectileId), item.PrefabName);
        Room.SendSyncEvent(hitEvent);
    }

    public override void Hit(string hitGoID)
    {
        var hit = new MeleeHit_SyncEvent(new SyncEvent(_gameObjectId, SyncEvent.EventType.MeleeHit, Room.Time));

        hit.EventDataList.Add(0);
        hit.EventDataList.Add(Position.X);
        hit.EventDataList.Add(Position.Y);

        Room.SendSyncEvent(hit);
        Room.RemoveProjectile(ProjectileId);
    }
}
