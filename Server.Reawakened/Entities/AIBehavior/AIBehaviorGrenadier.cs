namespace Server.Reawakened.Entities.AIBehavior;

public class AIBehaviorGrenadier(float inDuration, float loopDuration, float outDuration, bool isTracking, int projCount, float projSpeed, float maxHeight) : AIBaseBehavior
{
    public AI_Behavior_Grenadier GrenadierBehavior = new(inDuration, loopDuration, outDuration, isTracking, projCount, projSpeed, maxHeight);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => GrenadierBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => GrenadierBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => GrenadierBehavior.GetBehaviorRatio(aiData, roomTime);
}
