using AssetStudio;
using Server.Base.Core.Extensions;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorPatrol(BehaviorEnemy enemy, PatrolProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() =>
        new PatrolProperties(
            Fallback(enemy.Global.Patrol_MoveSpeed, fallback.patrol_MoveSpeed),
            Fallback(enemy.Global.Patrol_SmoothMove, fallback.patrol_SmoothMove),
            Fallback(enemy.Global.Patrol_EndPathWaitTime, fallback.patrol_EndPathWaitTime),
            Fallback(enemy.Generic.PatrolX, fallback.patrolDistanceX),
            Fallback(enemy.Generic.PatrolY, fallback.patrolDistanceY),
            Fallback(enemy.Generic.Patrol_ForceDirectionX, fallback.patrolForceDirectionX), 
            Fallback(enemy.Generic.Patrol_InitialProgressRatio, fallback.patrolInitialProgressRatio)
        );

    public override object[] GetStartArgs()
    {
        var behavior = _behaviour as AI_Behavior_Patrol;

        behavior.SetStats(_aiData);

        var method = behavior.GetMethod("CalculateProgressRatio");
        method.Invoke(behavior, [_aiData]);

        var ratio = behavior.GetField("Patrol_InitialProgressRatio");

        Logger.Error($"Should be sending {ratio} as the progress ratio, however - this is currently broken!");

        return [];
    }

    public override StateType GetStateType() => StateType.Patrol;

    public override void NextState() { }
}
