using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorBomber(BomberProperties properties, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float InTime => properties.inTime;
    public float LoopTime => properties.loopTime;
    public float BombRadius => properties.bombRadius;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Bomber;

    public override object[] GetProperties() => [InTime, LoopTime, BombRadius];

    public override object[] GetStartArgs() => [];

    public override void NextState() { }
}
