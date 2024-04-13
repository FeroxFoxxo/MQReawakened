using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorGoTo(BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() => new EmptyAiProperties();
    public override object[] GetStartArgs() => throw new NotImplementedException();
    public override void NextState() => throw new NotImplementedException();
}
