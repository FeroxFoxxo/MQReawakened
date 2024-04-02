using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorPatrol(PatrolState patrolState, AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) : AIBaseBehavior
{
    public float MoveSpeed => globalComp.Patrol_MoveSpeed != default ? globalComp.Patrol_MoveSpeed : patrolState.Speed;
    public bool SmoothMove => globalComp.Patrol_SmoothMove != default ? globalComp.Patrol_SmoothMove : patrolState.SmoothMove;
    public float EndPathWaitTime => globalComp.Patrol_EndPathWaitTime != default ? globalComp.Patrol_EndPathWaitTime : patrolState.EndPathWaitTime;
    public float PatrolX => genericComp.PatrolX;
    public float PatrolY => genericComp.PatrolY;
    public int ForceDirectionX => genericComp.Patrol_ForceDirectionX;
    public float InitialProgressRatio => genericComp.Patrol_InitialProgressRatio;

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Patrol(MoveSpeed, EndPathWaitTime, PatrolX, PatrolY, ForceDirectionX, InitialProgressRatio);

    public override StateTypes GetBehavior() => StateTypes.Patrol;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(MoveSpeed);
        sb.Append(SmoothMove ? 1 : 0);
        sb.Append(EndPathWaitTime);
        sb.Append(PatrolX);
        sb.Append(PatrolY);
        sb.Append(ForceDirectionX);
        sb.Append(InitialProgressRatio);

        return sb.ToString();
    }
}
