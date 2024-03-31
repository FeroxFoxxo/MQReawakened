using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Projectiles;
public class ChargeAttackProjectile : BaseProjectile
{
    private readonly TimerThread _timerThread;
    private readonly Player _player;

    private readonly int _itemId;
    private readonly int _zoneId;

    public override BaseCollider Collider { get; set; }

    public ChargeAttackProjectile(string id, Player player, Vector3Model startPosition, Vector3Model endPosition, Vector2Model speed, float lifeTime, int itemId, int zoneId, int damage, Elemental type, ServerRConfig config, TimerThread timerThread)
        : base(id, speed.X, speed.Y, lifeTime, player.Room, startPosition, endPosition, config)
    {
        _timerThread = timerThread;
        _player = player;
        _itemId = itemId;
        _zoneId = zoneId;

        Collider = new AttackCollider(player.GameObjectId, startPosition, 1, 1, PrjPlane, player, damage, type, 15f, 0);

        Room.SendSyncEvent(new ChargeAttackStart_SyncEvent(player.GameObjectId.ToString(), Room.Time,
                        endPosition.X, endPosition.Y, speed.X, speed.Y, itemId, zoneId));
    }

    public override void Hit(string hitGoID)
    {
        _player.TempData.IsSuperStomping = false;
        _player.TemporaryInvincibility(_timerThread, 1);

        Room.SendSyncEvent(new ChargeAttackStop_SyncEvent(_player.GameObjectId.ToString(), Room.Time,
           _player.TempData.Position.X, _player.TempData.Position.Y, _itemId, _zoneId, hitGoID));

        Room.RemoveProjectile(_player.GameObjectId);
    }
}
