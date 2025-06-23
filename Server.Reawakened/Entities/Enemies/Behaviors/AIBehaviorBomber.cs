using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorBomber(BomberProperties properties, BehaviorEnemy enemy) : AIBaseBehavior(enemy, StateType.Bomber)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => properties;

    public override object[] GetStartArgs() => [];

    public override void NextState() { }
}
