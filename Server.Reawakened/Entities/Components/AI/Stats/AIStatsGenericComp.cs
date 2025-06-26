using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.AI.Stats;
public class AIStatsGenericComp : Component<AI_Stats_Generic>
{
    public float Patrol_DistanceX => ComponentData.Patrol_DistanceX;
    public float Patrol_DistanceY => ComponentData.Patrol_DistanceY;
    public float Patrol_InitialProgressRatio => ComponentData.Patrol_InitialProgressRatio;
    public int Patrol_ForceDirectionX => ComponentData.Patrol_ForceDirectionX;
    public bool Aggro_UseAttackBeyondPatrolLine => ComponentData.Aggro_UseAttackBeyondPatrolLine;

    public float PatrolX { get; set; }
    public float PatrolY { get; set; }

    public override void InitializeComponent()
    {
        PatrolX = Patrol_DistanceX;
        PatrolY = Patrol_DistanceY;
    }

    public void SetPatrolRange(Vector3 patrol)
    {
        PatrolX = patrol.x;
        PatrolY = patrol.y;
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
    }
}
