using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
public class LookAroundState(float lookTime, int startDirection, int forceDirection, float initialProgressRatio,
    bool snapOnGround, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float LookTime => lookTime;
    public int StartDirection => startDirection;
    public int ForceDirection => forceDirection;
    public float InitialProgressRatio => initialProgressRatio;
    public bool SnapOnGround => snapOnGround;

    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorLookAround(this, enemy);
}
