namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Stinger(float speedForward, float speedBackward, float inDurationForward, float attackDuration, float damageAttackTimeOffset, float inDurationBackward) : AIBaseBehavior
{
    public AI_Behavior_Stinger StingerBehavior = new(speedForward, speedBackward, inDurationForward, attackDuration, damageAttackTimeOffset, inDurationBackward);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => StingerBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => StingerBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => StingerBehavior.GetBehaviorRatio(aiData, roomTime);
}
