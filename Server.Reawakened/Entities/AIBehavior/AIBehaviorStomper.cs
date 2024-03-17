namespace Server.Reawakened.Entities.AIBehavior;

public class AIBehaviorStomper(float attackTime, float impactTime) : AIBaseBehavior
{
    public AI_Behavior_Stomper StomperBehavior = new(attackTime, impactTime);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => StomperBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => StomperBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => StomperBehavior.GetBehaviorRatio(aiData, roomTime);

    public override string GetBehavior() => "Stomper";
}
