using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class BomberState(float inTime, float loopTime, float bombRadius, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float InTime { get; } = inTime;
    public float LoopTime { get; } = loopTime;
    public float BombRadius { get; } = bombRadius;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorBomber(this);
}
