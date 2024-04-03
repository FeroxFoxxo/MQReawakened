using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;

public class AggroState(float aggroSpeed, float moveBeyondTargetDistance, bool stayOnPatrolPath, float attackBeyondPatrolLine,
    bool useAttackBeyondPatrolLine, float detectionRangeUpY, float detectionRangeDownY, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float AggroSpeed => aggroSpeed;
    public float MoveBeyondTargetDistance => moveBeyondTargetDistance;
    public bool StayOnPatrolPath => stayOnPatrolPath;
    public float AttackBeyondPatrolLine => attackBeyondPatrolLine;
    public bool UseAttackBeyondPatrolLine => useAttackBeyondPatrolLine;
    public float DetectionRangeUpY => detectionRangeUpY;
    public float DetectionRangeDownY => detectionRangeDownY;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorAggro(this, globalComp);
}
