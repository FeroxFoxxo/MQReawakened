using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStomper(StomperProperties stomperState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float AttackTime => stomperState.attackDuration;
    public float ImpactTime => stomperState.impactTime;
    public float DamageDistance => stomperState.damageDistance;
    public float DamageOffset => stomperState.damageOffset;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Stomper;

    public override object[] GetProperties() => [ AttackTime ];

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(StateType.LookAround, Enemy.Position.x, Enemy.Position.y, Enemy.Generic.Patrol_ForceDirectionX);
}
