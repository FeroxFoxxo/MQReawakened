using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;

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

    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorAggro(this, enemy);
}
