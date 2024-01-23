namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Patrol : AIBaseBehavior
{
    public AI_Behavior_Patrol PatrolBehavior;

    public AIBehavior_Patrol(float patrolX, float patrolY, float patrolSpeed, float endPathWaitTime, int patrolForceDirectionX, float initialProgressRatio)
    {
        PatrolBehavior = new AI_Behavior_Patrol(patrolSpeed, endPathWaitTime, patrolX, patrolY, patrolForceDirectionX, initialProgressRatio);
    }

    public override bool Update(AIProcessData aiData, float roomTime)
    {
        return PatrolBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(AIProcessData aiData, float roomTime)
    {
        return PatrolBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
