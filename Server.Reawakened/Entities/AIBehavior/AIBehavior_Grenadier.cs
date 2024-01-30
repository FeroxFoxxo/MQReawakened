namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Grenadier : AIBaseBehavior
{
    public AI_Behavior_Grenadier GrenadierBehavior;

    public AIBehavior_Grenadier(float inDuration, float loopDuration, float outDuration, bool isTracking, int projCount, float projSpeed, float maxHeight)
    {
        GrenadierBehavior = new AI_Behavior_Grenadier(inDuration, loopDuration, outDuration, isTracking, projCount, projSpeed, maxHeight);
    }

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args)
    {
        GrenadierBehavior.Start(aiData, roomTime, args);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return GrenadierBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return GrenadierBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
