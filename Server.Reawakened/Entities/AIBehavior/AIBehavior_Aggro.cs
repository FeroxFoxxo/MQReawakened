namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Aggro : AIBaseBehavior
{
    public AI_Behavior_ComeBack ComeBackBehavior;

    public AIBehavior_Aggro(float speed)
    {
        ComeBackBehavior = new AI_Behavior_ComeBack(speed);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return ComeBackBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return ComeBackBehavior.GetBehaviorRatio(aiData, roomTime);
    }
}
