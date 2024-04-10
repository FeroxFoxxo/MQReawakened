using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
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

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
    }
}
