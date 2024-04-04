using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorStinger(StingerState stingerState) : AIBaseBehavior
{
    public float SpeedForward => stingerState.SpeedForward;
    public float SpeedBackward => stingerState.SpeedBackward;
    public float InDurationForward => stingerState.InDurationForward;
    public float AttackDuration => stingerState.AttackDuration;
    public float DamageAttackTimeOffset => stingerState.DamageAttackTimeOffset;
    public float InDurationBackward => stingerState.InDurationBackward;
    public float StingerDamageDistance => stingerState.StingerDamageDistance;

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Stinger(SpeedForward, SpeedBackward, InDurationForward, AttackDuration, DamageAttackTimeOffset, InDurationBackward);

    public override StateType GetBehavior() => StateType.Stinger;

    public override object[] GetData() => [
        SpeedForward, SpeedBackward,
        InDurationForward, AttackDuration,
        DamageAttackTimeOffset, InDurationBackward,
        StingerDamageDistance
    ];
}
