using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Projectiles.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Projectiles;
public class ChargeAttackProjectile : BaseProjectile
{
    private readonly ServerRConfig _config;
    private readonly Player _player;

    private readonly int _itemId;
    private readonly int _zoneId;

    public ChargeAttackProjectile(string id, Player player, Vector3Model startPosition, Vector3 endPosition, Vector2 speed,
        float lifeTime, ItemDescription item, int zoneId, ServerRConfig config, ItemCatalog itemCatalog)
        : base(id, lifeTime, player.Room, startPosition, speed, endPosition, false)
    {
        _config = config;
        _player = player;
        _itemId = item != null ? item.ItemId : -1;
        _zoneId = zoneId;

        var superStompDamage = (int)Math.Ceiling(GameFlow.StatisticData.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Player, player.Character.GlobalLevel) +
                                                 GameFlow.StatisticData.GetGlobalStat(Globals.StompDamageBonus) * 2);
        
        var itemEffects = item != null ? item.ItemEffects : new List<ItemEffect>
            { new(ItemEffectType.StompDamage, superStompDamage, 0) };
        var prefabName = item != null ? item.PrefabName : player.GameObjectId;

        Collider = new AttackCollider(id, Position, new RectModel(-0.4f, -0.5f, 0.8f, 0.8f),
            PrjPlane, player, item, itemEffects, prefabName, 15f, 0,
            player.Character.StatusEffects.HasEffect(ItemEffectType.Detect), itemCatalog);
        
        var syncEvent = new ChargeAttackStart_SyncEvent(new SyncEvent(
            _player.GameObjectId.ToString(), SyncEvent.EventType.ChargeAttackStart, Room.Time));
        
        syncEvent.EventDataList.Add(endPosition.x);
        syncEvent.EventDataList.Add(endPosition.y);
        syncEvent.EventDataList.Add(speed.x);
        syncEvent.EventDataList.Add(speed.y);

        if (_config.GameVersion > Core.Enums.GameVersion.vPets2012)
        {
            syncEvent.EventDataList.Add(_itemId);
            syncEvent.EventDataList.Add(zoneId);
        }

        Room.SendSyncEvent(syncEvent);
    }

    public override void Hit(string hitGoID)
    {
        if (hitGoID.Equals("0"))
            hitGoID = "-1";

        _player.TempData.IsSuperStomping = false;
        _player.TemporaryInvincibility(0.2);

        var syncEvent = new ChargeAttackStop_SyncEvent(new SyncEvent(
            _player.GameObjectId.ToString(), SyncEvent.EventType.ChargeAttackStop, Room.Time));
        syncEvent.EventDataList.Add(_player.TempData.Position.X);
        syncEvent.EventDataList.Add(_player.TempData.Position.Y);

        if (_config.GameVersion > Core.Enums.GameVersion.vPets2012)
        {
            syncEvent.EventDataList.Add(_itemId);
            syncEvent.EventDataList.Add(_zoneId);
        }

        syncEvent.EventDataList.Add(hitGoID);

        Room.SendSyncEvent(syncEvent);

        Room.RemoveProjectile(ProjectileId);

        Room.Logger.LogTrace("Charge Attack Projectile {ProjectileId} hit entity {HitGoID} and was removed from the room.",
            ProjectileId, hitGoID);
    }
}
