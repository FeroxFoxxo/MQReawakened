using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class PatrolState(float speed, bool smoothMove, float endPathWaitTime, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float Speed => speed;
    public bool SmoothMove => smoothMove;
    public float EndPathWaitTime => endPathWaitTime;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorPatrol(this, globalComp, genericComp);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];
}
