using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStinger(StingerState stingerState) : AIBaseBehavior
{
    public float SpeedForward => stingerState.SpeedForward;
    public float SpeedBackward => stingerState.SpeedBackward;
    public float InDurationForward => stingerState.InDurationForward;
    public float AttackDuration => stingerState.AttackDuration;
    public float DamageAttackTimeOffset => stingerState.DamageAttackTimeOffset;
    public float InDurationBackward => stingerState.InDurationBackward;
    public float StingerDamageDistance => stingerState.StingerDamageDistance;

    public override bool ShouldDetectPlayers => false;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Stinger(SpeedForward, SpeedBackward, InDurationForward, AttackDuration, DamageAttackTimeOffset, InDurationBackward);

    public override object[] GetData() => [
        SpeedForward, SpeedBackward,
        InDurationForward, AttackDuration,
        DamageAttackTimeOffset, InDurationBackward,
        StingerDamageDistance
    ];

    public override void NextState(BehaviorEnemy enemy) =>
        enemy.ChangeBehavior(enemy.GenericScript.AwareBehavior, enemy.Position.x, enemy.Position.y, enemy.AiData.Intern_Dir);
}
