using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class BomberState(float inTime, float loopTime, float bombRadius, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float InTime { get; } = inTime;
    public float LoopTime { get; } = loopTime;
    public float BombRadius { get; } = bombRadius;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorBomber(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(InTime);
        sb.Append(LoopTime);
        sb.Append(BombRadius);

        return sb.ToString();
    }
}
