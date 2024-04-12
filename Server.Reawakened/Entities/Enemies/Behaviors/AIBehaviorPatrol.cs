using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorPatrol(PatrolState patrolState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float MoveSpeed => Enemy.Global.Patrol_MoveSpeed != Enemy.Global.Default.Patrol_MoveSpeed ? Enemy.Global.Patrol_MoveSpeed : patrolState.Speed;
    public bool SmoothMove => Enemy.Global.Patrol_SmoothMove != Enemy.Global.Default.Patrol_SmoothMove ? Enemy.Global.Patrol_SmoothMove : patrolState.SmoothMove;
    public float EndPathWaitTime => Enemy.Global.Patrol_EndPathWaitTime != Enemy.Global.Default.Patrol_EndPathWaitTime ? Enemy.Global.Patrol_EndPathWaitTime : patrolState.EndPathWaitTime;
    public float PatrolDistanceX => Enemy.Generic.PatrolX;
    public float PatrolDistanceY => Enemy.Generic.PatrolY;
    public int ForceDirectionX => Enemy.Generic.Patrol_ForceDirectionX;
    public float InitialProgressRatio => Enemy.Generic.Patrol_InitialProgressRatio;

    public override bool ShouldDetectPlayers => true;

    public override StateType State => StateType.Patrol;

    public override object[] GetProperties() => [
        MoveSpeed, SmoothMove, EndPathWaitTime,
        PatrolDistanceX, PatrolDistanceY, ForceDirectionX, InitialProgressRatio
    ];

    public override object[] GetStartArgs() => [ InitialProgressRatio ];

    public override void NextState() { }
}
