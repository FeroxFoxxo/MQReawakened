using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class LookAroundState(float lookTime, float startDirection, float forceDirection, float initialProgressRatio, bool snapOnGround, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float LookTime { get; } = lookTime;
    public float StartDirection { get; } = startDirection;
    public float ForceDirection { get; } = forceDirection;
    public float InitialProgressRatio { get; } = initialProgressRatio;
    public bool SnapOnGround { get; } = snapOnGround;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorLookAround(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(LookTime);
        sb.Append(StartDirection);
        sb.Append(ForceDirection);
        sb.Append(InitialProgressRatio);
        sb.Append(SnapOnGround ? 1 : 0);

        return sb.ToString();
    }
}
