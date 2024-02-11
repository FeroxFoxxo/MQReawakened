namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_ComeBack(float speed) : AIBaseBehavior
{
    public AI_Behavior_ComeBack ComeBackBehavior = new(speed);

    public override void Start(ref AIProcessData aiData, float startTime, string[] args) => ComeBackBehavior.Start(aiData, startTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => ComeBackBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => ComeBackBehavior.GetBehaviorRatio(aiData, roomTime);

    public override bool MustDoComeback(AIProcessData aiData) => ComeBackBehavior.MustDoComeback(aiData, ComeBackBehavior);
}
