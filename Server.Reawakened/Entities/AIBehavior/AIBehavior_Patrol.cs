namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Patrol : AIBaseBehavior
{
    public AI_Behavior_Patrol PatrolBehavior;

    public AIBehavior_Patrol(float patrolX, float patrolY, float patrolSpeed, float endPathWaitTime, int patrolForceDirectionX, float initialProgressRatio)
    {
        PatrolBehavior = new AI_Behavior_Patrol(patrolSpeed, endPathWaitTime, patrolX, patrolY, patrolForceDirectionX, initialProgressRatio);
    }

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args)
    {
        PatrolBehavior.Start(aiData, roomTime, args);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return PatrolBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return PatrolBehavior.GetBehaviorRatio(aiData, roomTime);
    }

    public override void Stop(ref AIProcessData aiData)
    {
        PatrolBehavior.Stop(aiData);
    }
    public override void GetComebackPosition(AIProcessData aiData, ref float outPosX, ref float outPosY)
    {
        PatrolBehavior.GetComebackPosition(aiData, ref outPosX, ref outPosY);
    }
}
