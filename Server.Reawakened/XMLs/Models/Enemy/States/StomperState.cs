using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class StomperState(float attackTime, float impactTime, float damageDistance, float damageOffset, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float AttackTime => attackTime;
    public float ImpactTime => impactTime;
    public float DamageDistance => damageDistance;
    public float DamageOffset => damageOffset;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorStomper(this);
}
