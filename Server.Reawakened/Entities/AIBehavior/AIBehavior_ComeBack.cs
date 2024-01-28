namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_ComeBack : AIBaseBehavior
{
    public AI_Behavior_ComeBack ComeBackBehavior;

    public AIBehavior_ComeBack(float speed)
    {
        ComeBackBehavior = new AI_Behavior_ComeBack(speed);
    }

    public override void Start(ref AIProcessData aiData, float startTime, string[] args)
    {
        ComeBackBehavior.Start(aiData, startTime, args);
    }

    public override bool Update(ref AIProcessData aiData, float roomTime)
    {
        return ComeBackBehavior.Update(aiData, roomTime);
    }

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime)
    {
        return ComeBackBehavior.GetBehaviorRatio(aiData, roomTime);
    }

    public override bool MustDoComeback(AIProcessData aiData)
    {
        return ComeBackBehavior.MustDoComeback(aiData, ComeBackBehavior);
    }
}
