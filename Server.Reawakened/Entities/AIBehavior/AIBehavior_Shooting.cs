namespace Server.Reawakened.Entities.AIBehavior;
internal class AIBehavior_Shooting(int nbBullets, float fireSpreadAngle, float delay, float delayShotAnim, int nbFireRounds, float delayBtwnRounds, float startTime, float endTime, float prjSpeed, bool isClockwise, float startAngle) : AIBaseBehavior
{
    public AI_Behavior_Shooting ShootingBehavior = new(nbBullets, fireSpreadAngle, delay, delayShotAnim, nbFireRounds, delayBtwnRounds, startTime, endTime, prjSpeed, isClockwise, startAngle);

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => ShootingBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => ShootingBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => ShootingBehavior.GetBehaviorRatio(aiData, roomTime);
}
