using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseEnemyControllerComp<T> : Component<T>, IEnemyController where T : EnemyController
{
    public int OnKillRepPoints => ComponentData.OnKillRepPoints;
    public bool TopBounceImmune => ComponentData.TopBounceImmune;
    public string OnKillMessageReceiver => ComponentData.OnKillMessageReceiver;
    public int EnemyLevelOffset => ComponentData.EnemyLevelOffset;
    public string OnDeathTargetID => ComponentData.OnDeathTargetID;
    public string EnemyDisplayName => ComponentData.EnemyDisplayName;
    public int EnemyDisplayNameSize => ComponentData.EnemyDisplayNameSize;
    public Vector3 EnemyDisplayNamePosition => ComponentData.EnemyDisplayNamePosition;
    public Color EnemyDisplayNameColor => ComponentData.EnemyDisplayNameColor;
    public EnemyScalingType EnemyScalingType => ComponentData.EnemyScalingType;
    public bool CanAutoScale => ComponentData.CanAutoScale;
    public bool CanAutoScaleResistance => ComponentData.CanAutoScaleResistance;
    public bool CanAutoScaleDamage => ComponentData.CanAutoScaleDamage;
    public ItemEffectType EnemyEffectType => ComponentData.EnemyEffectType;

    public InternalEnemyData InternalEnemyData { get; set; }
    public IServiceProvider ServiceProvider { get; set; }
    public ILogger<IEnemyController> Logger { get; set; }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
    }

    public override void DelayedComponentInitialization()
    {
        if (ParentPlane.Equals("TemplatePlane"))
            return;

        CreateEnemy(Id, PrefabName);
    }

    public override void SendDelayedData(Player player)
    {
        if (Room.IsObjectKilled(Id))
            player.SendSyncEventToPlayer(new AiHealth_SyncEvent(Id, Room.Time, 0, 0, 0, 0, "now", false, false));
    }

    public BaseEnemy CreateEnemy(string id, string prefabName)
    {
        if (!InternalEnemyData.EnemyInfoCatalog.TryGetValue(prefabName, out var enemyModel))
        {
            Logger.LogError("Could not find enemy with name {EnemyPrefab}! Returning null...", prefabName);
            return null;
        }

        var enemyData = new EnemyData(Room, id, prefabName, this, enemyModel, ServiceProvider);

        BaseEnemy enemy = enemyModel.AiType switch
        {
            AiType.Behavior => new BehaviorEnemy(enemyData),
            AiType.State => new AIStateEnemy(enemyData),
            _ => null,
        };

        if (enemy != null)
            Room.AddEnemy(enemy);

        return enemy;
    }
}
