using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;
public class GenericProjectile : BaseProjectile
{
    private readonly string _gameObjectId;
    private readonly float _gravityFactor;

    public GenericProjectile(string id, Player player, float lifeTime, Vector3 position, ItemRConfig config,
        int direction, ItemDescription item, int damage, Elemental damageType, bool isGrenade, ServerRConfig rConfig)
            : base(id, lifeTime, player.Room,
                new Vector3Model(position.x + config.ProjectileXOffset * (direction > 0 ? 1 : - 1 - config.ProjectileWidth), position.y + config.ProjectileYOffset - config.ProjectileHeight, position.z),
                new Vector2(config.ProjectileSpeedX * (direction > 0 ? 1 : -1), isGrenade ? config.GrenadeSpeedY : config.ProjectileSpeedY),
                null, false)
    {
        _gameObjectId = player.GameObjectId;
        _gravityFactor = isGrenade ? config.GrenadeGravityFactor : config.ProjectileGravityFactor;

        Collider = new AttackCollider(id, Position, new RectModel(0, 0, config.ProjectileWidth, config.ProjectileHeight), PrjPlane, player, damage, damageType, LifeTime, 0, player.Character.StatusEffects.HasEffect(ItemEffectType.Detect));

        var prj = new LaunchItem_SyncEvent(new SyncEvent(_gameObjectId, SyncEvent.EventType.LaunchItem, StartTime));
        prj.EventDataList.Add(Position.X);
        prj.EventDataList.Add(Position.Y);
        prj.EventDataList.Add(Position.Z);
        prj.EventDataList.Add(Speed.x);
        prj.EventDataList.Add(Speed.y);
        prj.EventDataList.Add(LifeTime);
        prj.EventDataList.Add(int.Parse(ProjectileId));
        prj.EventDataList.Add(item.PrefabName);
        
        if (rConfig.GameVersion <= GameVersion.vMinigames2012)
        {
            prj.EventDataList.Add(1); // Projectile Strength
            prj.EventDataList.Add(isGrenade ? 1 : 0);
        }

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

        Room.Logger.LogTrace("Generic Projectile {ProjectileId} hit entity {HitGoID} and was removed from the room.",
            ProjectileId, hitGoID);
    }
}
