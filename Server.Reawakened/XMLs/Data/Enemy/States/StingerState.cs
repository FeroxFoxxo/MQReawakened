using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
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

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorStinger(this);

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
