namespace Server.Reawakened.Entities.AIBehavior;

public class AIBehaviorLookAround(float lookTime, float initialProgressRatio, bool snapOnGround) : AIBaseBehavior
{
    public AI_Behavior_LookAround LookAroundBehavior = new(lookTime, initialProgressRatio, snapOnGround);

    public override void Start(ref AIProcessData aiData, float startTime, string[] args) => LookAroundBehavior.Start(aiData, startTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => LookAroundBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => LookAroundBehavior.GetBehaviorRatio(aiData, roomTime);
}
