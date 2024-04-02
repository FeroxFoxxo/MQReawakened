using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
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

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorAggro(this, globalComp);
}
