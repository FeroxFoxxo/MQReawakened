using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorPatrol(PatrolState patrolState, AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) : AIBaseBehavior
{
    public float MoveSpeed => globalComp.Patrol_MoveSpeed != globalComp.Default.Patrol_MoveSpeed ? globalComp.Patrol_MoveSpeed : patrolState.Speed;
    public bool SmoothMove => globalComp.Patrol_SmoothMove != globalComp.Default.Patrol_SmoothMove ? globalComp.Patrol_SmoothMove : patrolState.SmoothMove;
    public float EndPathWaitTime => globalComp.Patrol_EndPathWaitTime != globalComp.Default.Patrol_EndPathWaitTime ? globalComp.Patrol_EndPathWaitTime : patrolState.EndPathWaitTime;
    public float PatrolX => genericComp.PatrolX;
    public float PatrolY => genericComp.PatrolY;
    public int ForceDirectionX => genericComp.Patrol_ForceDirectionX;
    public float InitialProgressRatio => genericComp.Patrol_InitialProgressRatio;

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Patrol(MoveSpeed, EndPathWaitTime, PatrolX, PatrolY, ForceDirectionX, InitialProgressRatio);

    public override StateType GetBehavior() => StateType.Patrol;

    public override object[] GetData() => [
        MoveSpeed, SmoothMove, EndPathWaitTime,
        PatrolX, PatrolY, ForceDirectionX, InitialProgressRatio
    ];
}
