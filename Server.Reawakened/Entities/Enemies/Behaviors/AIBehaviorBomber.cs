using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorBomber(BomberState bomberState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float InTime => bomberState.InTime;
    public float LoopTime => bomberState.LoopTime;
    public float BombRadius => bomberState.BombRadius;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Bomber;

    public override object[] GetProperties() => [InTime, LoopTime, BombRadius];

    public override object[] GetStartArgs() => [];

    public override void NextState() { }
}
