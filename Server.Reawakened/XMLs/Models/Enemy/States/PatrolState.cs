using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class PatrolState(float speed, bool smoothMove, float endPathWaitTime, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float Speed { get; } = speed;
    public bool SmoothMove { get; } = smoothMove;
    public float EndPathWaitTime { get; } = endPathWaitTime;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorPatrol(this, generic);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(Speed);
        sb.Append(SmoothMove ? 1 : 0);
        sb.Append(EndPathWaitTime);
        sb.Append(generic.PatrolX);
        sb.Append(generic.PatrolY);
        sb.Append(generic.Patrol_ForceDirectionX);
        sb.Append(generic.Patrol_InitialProgressRatio);

        return sb.ToString();
    }
}
