using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class LookAroundState(float lookTime, float startDirection, float forceDirection, float initialProgressRatio, bool snapOnGround, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float LookTime => lookTime;
    public float StartDirection => startDirection;
    public float ForceDirection => forceDirection;
    public float InitialProgressRatio => initialProgressRatio;
    public bool SnapOnGround => snapOnGround;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorLookAround(this, globalComp);
}
