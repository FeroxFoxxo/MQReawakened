using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
public class LookAroundState(float lookTime, float startDirection, float forceDirection, float initialProgressRatio, bool snapOnGround, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float LookTime => lookTime;
    public float StartDirection => startDirection;
    public float ForceDirection => forceDirection;
    public float InitialProgressRatio => initialProgressRatio;
    public bool SnapOnGround => snapOnGround;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorLookAround(this, globalComp);
}
