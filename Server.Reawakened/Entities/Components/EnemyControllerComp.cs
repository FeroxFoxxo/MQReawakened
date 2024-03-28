using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;
using UnityEngine;
using Room = Server.Reawakened.Rooms.Room;

namespace Server.Reawakened.Entities.Components;
public class EnemyControllerComp : Component<EnemyController>, IDestructible
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

    public InternalDefaultEnemies EnemyInfoXml { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public TimerThread TimerThread { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ILogger<EnemyControllerComp> Logger { get; set; }
    public ServerRConfig Config { get; set; }

    public int Level;
    public int EnemyHealth;
    public int MaxHealth;
    public int OnKillExp;

    private int _damage;

    public override void InitializeComponent()
    {
        Level = Room.LevelInfo.Difficulty + EnemyLevelOffset;
        MaxHealth = WorldStatistics.GetValue(ItemEffectType.IncreaseHitPoints, WorldStatisticsGroup.Enemy, Level);
        EnemyHealth = MaxHealth;
        OnKillExp = WorldStatistics.GetValue(ItemEffectType.IncreaseExperience, WorldStatisticsGroup.Enemy, Level);
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        _damage = WorldStatistics.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, Level);

        if (_damage < 0)
            _damage = 1;

        player.ApplyCharacterDamage(_damage, 1, TimerThread);

        return;
    }

    public void Damage(int damage, Player origin)
    {
        if (Room.IsObjectKilled(Id))
            return;

        EnemyHealth -= damage;

        var breakEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, EnemyHealth, damage, 0, 0, origin.CharacterName, false, true);
        origin.Room.SendSyncEvent(breakEvent);

        if (EnemyHealth <= 0)
        {
            origin.AddReputation(OnKillExp, Config);
            Room.KillEntity(origin, Id);
        }
    }

    public void Destroy(Player player, Room room, string id)
    {
        player.CheckObjective(ObjectiveEnum.Score, id, PrefabName, 1, QuestCatalog);
        player.CheckObjective(ObjectiveEnum.Scoremultiple, id, PrefabName, 1, QuestCatalog);

        player.CheckAchievement(AchConditionType.DefeatEnemy, string.Empty, InternalAchievement, Logger);
        player.CheckAchievement(AchConditionType.DefeatEnemy, PrefabName, InternalAchievement, Logger);
        player.CheckAchievement(AchConditionType.DefeatEnemyInLevel, player.Room.LevelInfo.Name, InternalAchievement, Logger);

        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
