using A2m.Server;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public interface IEnemyController
{
    public string Name { get; }
    public string PrefabName { get; }
    public string ParentPlane { get; }
    public RectModel Rectangle { get; }
    public Vector3Model Position { get; }
    public Vector3Model Rotation { get; }
    public Vector3Model Scale { get; }

    public int OnKillRepPoints { get; }
    public bool TopBounceImmune { get; }
    public string OnKillMessageReceiver { get; }
    public int EnemyLevelOffset { get; }
    public string OnDeathTargetID { get; }
    public string EnemyDisplayName { get; }
    public int EnemyDisplayNameSize { get; }
    public Vector3 EnemyDisplayNamePosition { get; }
    public Color EnemyDisplayNameColor { get; }
    public EnemyScalingType EnemyScalingType { get; }
    public bool CanAutoScale { get; }
    public bool CanAutoScaleResistance { get; }
    public bool CanAutoScaleDamage { get; }
    public ItemEffectType EnemyEffectType { get; }
    public BaseEnemy CreateEnemy(string id, string prefabName);
}
