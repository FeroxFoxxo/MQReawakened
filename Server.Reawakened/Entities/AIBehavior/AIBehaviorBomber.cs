namespace Server.Reawakened.Entities.AIBehavior;

public class AIBehaviorBomber(float inTime, float loopTime) : AIBaseBehavior
{
    public AI_Behavior_Bomber BomberBehavior = new(inTime, loopTime);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => BomberBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => BomberBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => BomberBehavior.GetBehaviorRatio(aiData, roomTime);

    public override string GetBehavior() => "Bomber";
}
