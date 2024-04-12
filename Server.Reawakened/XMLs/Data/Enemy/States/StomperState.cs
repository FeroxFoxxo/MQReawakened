using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
public class StomperState(float attackTime, float impactTime, float damageDistance, float damageOffset, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float AttackTime => attackTime;
    public float ImpactTime => impactTime;
    public float DamageDistance => damageDistance;
    public float DamageOffset => damageOffset;

    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorStomper(this, enemy);
}
