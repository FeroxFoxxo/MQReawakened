using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class AggroState(float aggroSpeed, float moveBeyondTargetDistance, bool stayOnPatrolPath, float attackBeyondPatrolLine,
    bool useAttackBeyondPatrolLine, float detectionRangeUpY, float detectionRangeDownY, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float AggroSpeed { get; } = aggroSpeed;
    public float MoveBeyondTargetDistance { get; } = moveBeyondTargetDistance;
    public bool StayOnPatrolPath { get; } = stayOnPatrolPath;
    public float AttackBeyondPatrolLine { get; } = attackBeyondPatrolLine;
    public bool UseAttackBeyondPatrolLine { get; } = useAttackBeyondPatrolLine;
    public float DetectionRangeUpY { get; } = detectionRangeUpY;
    public float DetectionRangeDownY { get; } = detectionRangeDownY;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorAggro(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(AggroSpeed);
        sb.Append(MoveBeyondTargetDistance);
        sb.Append(StayOnPatrolPath ? 1 : 0);
        sb.Append(AttackBeyondPatrolLine);
        sb.Append(UseAttackBeyondPatrolLine ? 1 : 0);
        sb.Append(DetectionRangeUpY);
        sb.Append(DetectionRangeDownY);

        return sb.ToString();
    }
}
