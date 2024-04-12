using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStomper(StomperState stomperState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float AttackTime => stomperState.AttackTime;
    public float ImpactTime => stomperState.ImpactTime;
    public float DamageDistance => stomperState.DamageDistance;
    public float DamageOffset => stomperState.DamageOffset;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Stomper;

    public override object[] GetProperties() => [
        AttackTime
    ];

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(StateType.LookAround, Enemy.Position.x, Enemy.Position.y, Enemy.Generic.Patrol_ForceDirectionX);
}
