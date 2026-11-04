using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;

public class DamageZoneEntity : BaseProjectile
{
    private readonly string _gameObjectId;
    private int lastTick = 0;
    private int currentTick = 1;

    public DamageZoneEntity(string id, Vector3Model position, Player player, int direction, ItemDescription item, ItemCatalog itemCatalog)
        : base(id, 10, player.Room, position, new Vector2(0, 0), null, false)
    {
        _gameObjectId = player.GameObjectId;

        var isRight = direction > 0;

        var horizontalOffset = isRight ? -0.2f : -3f;

        var left = horizontalOffset;
        var top = 0f;
        var width = 3.2f;
        var height = 2.5f;

        Collider = new AttackCollider(
            id, 
            Position, 
            new RectModel(left, top, width, height),
            PrjPlane,
            player,
            item,
            item.ItemEffects,
            item.PrefabName,
            10f,
            0f, 
            player.Character.StatusEffects.HasEffect(ItemEffectType.Detect),
            itemCatalog,
            0.5f
        );

        var zoneEvent = new DamageZone_SyncEvent(
            _gameObjectId, Room.Time,
            position.X + (isRight ? 2f : -2f), Position.Y, Position.Z, direction, Speed.y, LifeTime,
        int.Parse(ProjectileId), item.PrefabName
        );

        Room.SendSyncEvent(zoneEvent);
    }

    public override void Update()
    {
        if (Room == null) return;

        currentTick = (int)(Room.Time - StartTime);

        if (currentTick > lastTick)
        {
            lastTick = currentTick;
            RunCollisionCheck();
        }

        if (LifeTime <= Room.Time)
            Room.RemoveProjectile(ProjectileId);
    }

    public override void Hit(string hitGoID)
    {
        Room.Logger.LogTrace("DamageZone Projectile {ProjectileId} hit entity {HitGoID}.",
            ProjectileId, hitGoID);
    }
}
