using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
public class StingerState(float speedForward, float speedBackward, float inDurationForward, float attackDuration,
    float damageAttackTimeOffset, float inDurationBackward, float stingerDamageDistance, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float SpeedForward => speedForward;
    public float SpeedBackward => speedBackward;
    public float InDurationForward => inDurationForward;
    public float AttackDuration => attackDuration;
    public float DamageAttackTimeOffset => damageAttackTimeOffset;
    public float InDurationBackward => inDurationBackward;
    public float StingerDamageDistance => stingerDamageDistance;

    public override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorStinger(this);

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
}
