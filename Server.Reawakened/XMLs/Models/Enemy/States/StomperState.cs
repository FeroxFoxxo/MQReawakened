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

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorStomper(this);
}
