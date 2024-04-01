using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorShooting(ShootingState shootingState) : AIBaseBehavior
{
    public AI_Behavior_Shooting ShootingBehavior = new(
        shootingState.NbBulletsPerRound, shootingState.FireSpreadAngle,
        shootingState.DelayBetweenBullet, shootingState.DelayShootAnim,
        shootingState.NbFireRounds, shootingState.DelayBetweenFireRound,
        shootingState.StartCoolDownTime, shootingState.EndCoolDownTime,
        shootingState.ProjectileSpeed, shootingState.FireSpreadClockwise,
        shootingState.FireSpreadStartAngle
    );

    public override void Start(ref AIProcessData aiData, float roomTime, string[] args) => ShootingBehavior.Start(aiData, roomTime, args);

    public override bool Update(ref AIProcessData aiData, float roomTime) => ShootingBehavior.Update(aiData, roomTime);

    public override float GetBehaviorRatio(ref AIProcessData aiData, float roomTime) => ShootingBehavior.GetBehaviorRatio(aiData, roomTime);

    public override StateTypes GetBehavior() => StateTypes.Shooting;
}
