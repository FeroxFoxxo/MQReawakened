using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;
public class ChargeAttackProjectile : BaseProjectile
{
    private readonly ServerRConfig _config;
    private readonly TimerThread _timerThread;
    private readonly Player _player;

    private readonly int _itemId;
    private readonly int _zoneId;

    public ChargeAttackProjectile(string id, Player player, Vector3 startPosition, Vector3 endPosition, Vector2 speed, float lifeTime, int itemId, int zoneId, int damage, Elemental type, ServerRConfig config, TimerThread timerThread)
        : base(id, lifeTime, player.Room, startPosition, speed, endPosition, false, config)
    {
        _config = config;
        _timerThread = timerThread;
        _player = player;
        _itemId = itemId;
        _zoneId = zoneId;

        Collider = new AttackCollider(player.GameObjectId, startPosition, new Rect(0, 0, 1, 1), PrjPlane, player, damage, type, 15f, 0, player.Character.StatusEffects.HasEffect(ItemEffectType.Detect));

        Room.SendSyncEvent(new ChargeAttackStart_SyncEvent(player.GameObjectId.ToString(), Room.Time,
                        endPosition.x, endPosition.y, speed.x, speed.y, itemId, zoneId));
    }

    public override void Hit(string hitGoID)
    {
        _player.TempData.IsSuperStomping = false;
        _player.TemporaryInvincibility(_timerThread, _config, 1);

        Room.SendSyncEvent(
            new ChargeAttackStop_SyncEvent(
                _player.GameObjectId.ToString(), Room.Time,
                _player.TempData.Position.x, _player.TempData.Position.y, _itemId, _zoneId, hitGoID
            )
        );

        Room.RemoveProjectile(_player.GameObjectId);
    }
}
