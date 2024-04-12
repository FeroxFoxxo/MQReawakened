using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorIdle(BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public override bool ShouldDetectPlayers => true;

    public override StateType State => StateType.Idle;

    public override object[] GetProperties() => [];

    public override object[] GetStartArgs() => [];

    public override void NextState() { }
}
