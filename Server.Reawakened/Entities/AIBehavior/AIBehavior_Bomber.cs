namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Bomber : AIBaseBehavior
{
    public AI_Behavior_Bomber BomberBehavior;

    public AIBehavior_Bomber(float inTime, float loopTime)
    {
        BomberBehavior = new AI_Behavior_Bomber(inTime, loopTime);
    }

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args)
    {
        BomberBehavior.Start(aiData, roomTime, args);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return BomberBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return BomberBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
