namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Aggro : AIBaseBehavior
{
    public AI_Behavior_Aggro AggroBehavior;

    public AIBehavior_Aggro(float attackSpeed, float moveBeyondTargetDistance, bool stayOnPatrolPath, float attackBeyondPatrolLine, float detectionHeightUp, float detectionHeightDown)
    {
        AggroBehavior = new AI_Behavior_Aggro(attackSpeed, moveBeyondTargetDistance, stayOnPatrolPath, attackBeyondPatrolLine, detectionHeightUp, detectionHeightDown);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return AggroBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return AggroBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
