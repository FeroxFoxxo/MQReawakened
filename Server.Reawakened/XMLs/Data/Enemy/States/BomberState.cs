using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class BomberState(float inTime, float loopTime, float bombRadius, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float InTime => inTime;
    public float LoopTime => loopTime;
    public float BombRadius => bombRadius;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorBomber(this);
}
