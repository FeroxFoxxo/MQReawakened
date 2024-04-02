using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class PatrolState(float speed, bool smoothMove, float endPathWaitTime, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float Speed { get; } = speed;
    public bool SmoothMove { get; } = smoothMove;
    public float EndPathWaitTime { get; } = endPathWaitTime;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorPatrol(this, globalComp, genericComp);
}
