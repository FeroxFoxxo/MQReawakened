using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class StomperState(float attackTime, float impactTime, float damageDistance, float damageOffset, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float AttackTime { get; } = attackTime;
    public float ImpactTime { get; } = impactTime;
    public float DamageDistance { get; } = damageDistance;
    public float DamageOffset { get; } = damageOffset;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorStomper(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(AttackTime);
        sb.Append(ImpactTime);
        sb.Append(DamageDistance);
        sb.Append(DamageOffset);

        return sb.ToString();
    }
}
