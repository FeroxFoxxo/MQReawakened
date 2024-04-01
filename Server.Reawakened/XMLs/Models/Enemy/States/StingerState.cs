using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class StingerState(float speedForward, float speedBackward, float inDurationForward, float attackDuration,
    float damageAttackTimeOffset, float inDurationBackward, float stingerDamageDistance, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float SpeedForward { get; } = speedForward;
    public float SpeedBackward { get; } = speedBackward;
    public float InDurationForward { get; } = inDurationForward;
    public float AttackDuration { get; } = attackDuration;
    public float DamageAttackTimeOffset { get; } = damageAttackTimeOffset;
    public float InDurationBackward { get; } = inDurationBackward;
    public float StingerDamageDistance { get; } = stingerDamageDistance;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorStinger(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) =>
        [
            behaviorEnemy.AiData.Sync_TargetPosX.ToString(),
            behaviorEnemy.AiData.Sync_TargetPosY.ToString(),
            "0",
            behaviorEnemy.AiData.Intern_SpawnPosX.ToString(),
            behaviorEnemy.AiData.Intern_SpawnPosY.ToString(),
            behaviorEnemy.AiData.Intern_SpawnPosZ.ToString(),
            SpeedForward.ToString(),
            SpeedBackward.ToString()
        ];

    public override string ToStateString(AIStatsGenericComp generic)
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(SpeedForward);
        sb.Append(SpeedBackward);
        sb.Append(InDurationForward);
        sb.Append(AttackDuration);
        sb.Append(DamageAttackTimeOffset);
        sb.Append(InDurationBackward);
        sb.Append(StingerDamageDistance);

        return sb.ToString();
    }
}
