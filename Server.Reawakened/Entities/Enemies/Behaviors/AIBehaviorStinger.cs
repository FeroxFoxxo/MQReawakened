using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStinger(StingerState stingerState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float SpeedForward => stingerState.SpeedForward;
    public float SpeedBackward => stingerState.SpeedBackward;
    public float InDurationForward => stingerState.InDurationForward;
    public float AttackDuration => stingerState.AttackDuration;
    public float DamageAttackTimeOffset => stingerState.DamageAttackTimeOffset;
    public float InDurationBackward => stingerState.InDurationBackward;
    public float StingerDamageDistance => stingerState.StingerDamageDistance;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Stinger;

    public override object[] GetProperties() => [
        SpeedForward, SpeedBackward,
        InDurationForward, AttackDuration,
        DamageAttackTimeOffset, InDurationBackward
    ];

    public override object[] GetStartArgs() =>
        [
            Enemy.AiData.Sync_TargetPosX,
            Enemy.AiData.Sync_TargetPosY,
            0, // Target will always be on first plane
            Enemy.AiData.Intern_SpawnPosX,
            Enemy.AiData.Intern_SpawnPosY,
            Enemy.AiData.Intern_SpawnPosY,
            SpeedForward,
            SpeedBackward
        ];

    public override void NextState() =>
        Enemy.ChangeBehavior(Enemy.GenericScript.AwareBehavior, Enemy.Position.x, Enemy.Position.y, Enemy.AiData.Intern_Dir);
}
