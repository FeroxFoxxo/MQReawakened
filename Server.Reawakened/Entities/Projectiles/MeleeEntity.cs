using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;

public class MeleeEntity : BaseProjectile
{
    private readonly string _gameObjectId;

    public MeleeEntity(string id, Vector3Model position, Player player, int direction, float lifeTime, ItemDescription item, int damage, Elemental type, ItemRConfig config,
        ServerRConfig serverRConfig)
        : base(id, lifeTime, player.Room, position, new Vector2(0, 0), null, false)
    {
        _gameObjectId = player.GameObjectId;

        var onGround = player.TempData.OnGround;
        var isRight = direction > 0;

        var meleeLeft = onGround ? isRight ? 0 : -config.MeleeWidth : -config.MeleeAerialOffset;
        var meleeTop = onGround ? 0 : -config.MeleeAerialOffset;
        var meleeWidth = onGround ? config.MeleeWidth : config.MeleeAerialRange;
        var meleeHeight = onGround ? config.MeleeHeight : config.MeleeAerialRange;

        Collider = new AttackCollider(id, Position, new RectModel(meleeLeft, meleeTop, meleeWidth, meleeHeight),
            PrjPlane, player, damage, type, LifeTime, onGround ? 0.1f : 0.5f, player.Character.StatusEffects.HasEffect(ItemEffectType.Detect)
        );

        var hitEvent = new Melee_SyncEvent(new SyncEvent(player.GameObjectId.ToString(), SyncEvent.EventType.Melee, Room.Time));
        hitEvent.EventDataList.Add(lifeTime);
        hitEvent.EventDataList.Add(Position.X);
        hitEvent.EventDataList.Add(Position.Y);
        hitEvent.EventDataList.Add(Position.Z);
        hitEvent.EventDataList.Add(item.PrefabName);

        if (serverRConfig.GameVersion <= GameVersion.vMinigames2012)
            hitEvent.EventDataList.Add(1); // Attack Strength

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

        Room.Logger.LogTrace("Melee Projectile {ProjectileId} hit entity {HitGoID} and was removed from the room.",
            ProjectileId, hitGoID);
    }
}
