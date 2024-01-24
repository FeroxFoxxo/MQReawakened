using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.BundlesInternal;
using UnityEngine;

namespace Server.Reawakened.Entities.Components;
public class EnemyControllerComp : Component<EnemyController>
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

    public ILogger<EnemyControllerComp> Logger { get; set; }
    public InternalDefaultEnemies EnemyInfoXml { get; set; }

    public int Level;
    public override void InitializeComponent()
    {
        Level = Room.LevelInfo.Difficulty + EnemyLevelOffset;
    }
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) => base.RunSyncedEvent(syncEvent, player);
}
