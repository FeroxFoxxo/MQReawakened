using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorIdle : AIBaseBehavior
{
    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Idle();

    public override bool ShouldDetectPlayers => true;

    public override object[] GetData() => [];

    public override void NextState(BehaviorEnemy enemy) { }
}
