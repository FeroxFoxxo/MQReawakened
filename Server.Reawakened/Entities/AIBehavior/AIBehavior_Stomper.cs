namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Stomper : AIBaseBehavior
{
    public AI_Behavior_Stomper StomperBehavior;

    public AIBehavior_Stomper(float attackTime, float impactTime)
    {
        StomperBehavior = new AI_Behavior_Stomper(attackTime, impactTime);
    }

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args)
    {
        StomperBehavior.Start(aiData, roomTime, args);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return StomperBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return StomperBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
