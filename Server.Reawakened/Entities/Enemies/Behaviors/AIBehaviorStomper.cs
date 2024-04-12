using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStomper(StomperState stomperState) : AIBaseBehavior
{
    public float AttackTime => stomperState.AttackTime;
    public float ImpactTime => stomperState.ImpactTime;
    public float DamageDistance => stomperState.DamageDistance;
    public float DamageOffset => stomperState.DamageOffset;

    public override bool ShouldDetectPlayers => false;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Stomper(AttackTime, ImpactTime);

    public override object[] GetData() => [
        AttackTime
    ];

    public override void NextState(BehaviorEnemy enemy) =>
        enemy.ChangeBehavior(StateType.LookAround, enemy.Position.x, enemy.Position.y, enemy.Generic.Patrol_ForceDirectionX);
}
