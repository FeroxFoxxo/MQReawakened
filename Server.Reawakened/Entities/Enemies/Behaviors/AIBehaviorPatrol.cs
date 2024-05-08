using AssetStudio;
using Server.Base.Core.Extensions;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorPatrol(PatrolProperties properties, BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() =>
        new PatrolProperties(
            Enemy.Global.Patrol_MoveSpeed != Enemy.Global.Default.Patrol_MoveSpeed ? Enemy.Global.Patrol_MoveSpeed : properties.patrol_MoveSpeed,
            Enemy.Global.Patrol_SmoothMove != Enemy.Global.Default.Patrol_SmoothMove ? Enemy.Global.Patrol_SmoothMove : properties.patrol_SmoothMove,
            Enemy.Global.Patrol_EndPathWaitTime != Enemy.Global.Default.Patrol_EndPathWaitTime ? Enemy.Global.Patrol_EndPathWaitTime : properties.patrol_EndPathWaitTime,
            Enemy.Generic.PatrolX,
            Enemy.Generic.PatrolY,
            Enemy.Generic.Patrol_ForceDirectionX,
            Enemy.Generic.Patrol_InitialProgressRatio
        );

    public override object[] GetStartArgs()
    {
        var behavior = Behavior as AI_Behavior_Patrol;

        behavior.SetStats(Enemy.AiData);

        var method = behavior.GetMethod("CalculateProgressRatio");
        method.Invoke(behavior, [Enemy.AiData]);

        var ratio = behavior.GetField("Patrol_InitialProgressRatio");

        Logger.Error($"Should be sending {ratio} as the progress ratio, however - this is currently broken!");

        return [];
    }

    public override void NextState() { }
}
