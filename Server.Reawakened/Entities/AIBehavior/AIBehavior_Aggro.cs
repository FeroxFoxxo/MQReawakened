namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Aggro(float attackSpeed, float moveBeyondTargetDistance, bool stayOnPatrolPath, float attackBeyondPatrolLine, float detectionHeightUp, float detectionHeightDown) : AIBaseBehavior
{
    public AI_Behavior_Aggro AggroBehavior = new(attackSpeed, moveBeyondTargetDistance, stayOnPatrolPath, attackBeyondPatrolLine, detectionHeightUp, detectionHeightDown);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => AggroBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => AggroBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => AggroBehavior.GetBehaviorRatio(aiData, roomTime);
}
