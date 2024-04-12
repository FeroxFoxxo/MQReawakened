using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;

public class PatrolState(float speed, bool smoothMove, float endPathWaitTime, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float Speed => speed;
    public bool SmoothMove => smoothMove;
    public float EndPathWaitTime => endPathWaitTime;

    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorPatrol(this, enemy);
}
