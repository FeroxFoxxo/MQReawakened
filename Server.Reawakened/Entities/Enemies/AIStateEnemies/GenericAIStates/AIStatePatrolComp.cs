using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.AIStateEnemies.GenericAIStates;
public class AIStatePatrolComp : Component<AIStatePatrol>
{
    public Vector2 Patrol1 => ComponentData.Patrol1;
    public Vector2 Patrol2 => ComponentData.Patrol2;
    public int SinusPathNbHalfPeriod => ComponentData.SinusPathNbHalfPeriod;
    public float MovementSpeed => ComponentData.MovementSpeed;
    public float IdleDurationAtTurnAround => ComponentData.IdleDurationAtTurnAround;
    public float DetectionRange => ComponentData.DetectionRange;
    public float MinimumRange => ComponentData.MinimumRange;
    public float MinimumTimeBeforeDetection => ComponentData.MinimumTimeBeforeDetection;
    public float MaximumYDifferenceOnDetection => ComponentData.MaximumYDifferenceOnDetection;
    public bool RayCastDetection => ComponentData.RayCastDetection;
    public bool DetectOnlyInPatrolZone => ComponentData.DetectOnlyInPatrolZone;
    public float PatrolZoneSizeOffset => ComponentData.PatrolZoneSizeOffset;
}
