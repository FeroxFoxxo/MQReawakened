namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_LookAround : AIBaseBehavior
{
    public AI_Behavior_LookAround LookAroundBehavior;

    public AIBehavior_LookAround(float lookTime, float initialProgressRatio, bool snapOnGround)
    {
        LookAroundBehavior = new AI_Behavior_LookAround(lookTime, initialProgressRatio, snapOnGround);
    }

    public override void Start(ref AIProcessData aiData, float startTime, string[] args)
    {
        LookAroundBehavior.Start(aiData, startTime, args);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return LookAroundBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return LookAroundBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
