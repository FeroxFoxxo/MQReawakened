namespace Server.Reawakened.Entities.AIBehavior;

public class AIBehaviorPatrol(float patrolX, float patrolY, float patrolSpeed, float endPathWaitTime, int patrolForceDirectionX, float initialProgressRatio) : AIBaseBehavior
{
    public AI_Behavior_Patrol PatrolBehavior = new(patrolSpeed, endPathWaitTime, patrolX, patrolY, patrolForceDirectionX, initialProgressRatio);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => PatrolBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => PatrolBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => PatrolBehavior.GetBehaviorRatio(aiData, roomTime);

    public override void Stop(ref AIProcessData aiData) => PatrolBehavior.Stop(aiData);
    public override void GetComebackPosition(AIProcessData aiData, ref float outPosX, ref float outPosY) => PatrolBehavior.GetComebackPosition(aiData, ref outPosX, ref outPosY);
}
