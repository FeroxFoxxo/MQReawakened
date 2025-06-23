using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorIdle(BehaviorEnemy enemy) : AIBaseBehavior(enemy, StateType.Idle)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() => new EmptyAiProperties();

    public override object[] GetStartArgs() => [];

    public override void NextState() { }
}
