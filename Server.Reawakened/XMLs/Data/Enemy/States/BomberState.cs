using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;

public class BomberState(float inTime, float loopTime, float bombRadius, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float InTime => inTime;
    public float LoopTime => loopTime;
    public float BombRadius => bombRadius;

    public override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorBomber(this);
}
