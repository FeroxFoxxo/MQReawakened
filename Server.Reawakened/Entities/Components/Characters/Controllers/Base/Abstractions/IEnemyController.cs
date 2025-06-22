using A2m.Server;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public interface IEnemyController
{
    string Name { get; }
    string PrefabName { get; }
    string ParentPlane { get; }
    RectModel Rectangle { get; }
    Vector3Model Position { get; }
    Vector3Model Rotation { get; }
    Vector3Model Scale { get; }

    int OnKillRepPoints { get; }
    bool TopBounceImmune { get; }
    string OnKillMessageReceiver { get; }
    int EnemyLevelOffset { get; }
    string OnDeathTargetID { get; }
    string EnemyDisplayName { get; }
    int EnemyDisplayNameSize { get; }
    Vector3 EnemyDisplayNamePosition { get; }
    Color EnemyDisplayNameColor { get; }
    EnemyScalingType EnemyScalingType { get; }
    bool CanAutoScale { get; }
    bool CanAutoScaleResistance { get; }
    bool CanAutoScaleDamage { get; }
    ItemEffectType EnemyEffectType { get; }
    BaseEnemy CreateEnemy(string id, string prefabName);
}
