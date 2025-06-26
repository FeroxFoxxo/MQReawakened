using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorShooting(BehaviorEnemy enemy, ShootingProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() =>
        new ShootingProperties(
            Fallback(enemy.Global.Shooting_NbBulletsPerRound, fallback.shooting_NbBulletsPerRound),
            Fallback(enemy.Global.Shooting_FireSpreadAngle, fallback.shooting_FireSpreadAngle),
            Fallback(enemy.Global.Shooting_DelayBetweenBullet, fallback.shooting_DelayBetweenBullet),
            Fallback(enemy.Global.Shooting_DelayShot_Anim, fallback.shooting_DelayShoot_Anim),
            Fallback(enemy.Global.Shooting_NbFireRounds, fallback.shooting_NbFireRounds),
            Fallback(enemy.Global.Shooting_DelayBetweenFireRound, fallback.shooting_DelayBetweenFireRound),
            Fallback(enemy.Global.Shooting_StartCoolDownTime, fallback.shooting_StartCoolDownTime),
            Fallback(enemy.Global.Shooting_EndCoolDownTime, fallback.shooting_EndCoolDownTime),
            Fallback(enemy.Global.Shooting_ProjectileSpeed, fallback.shooting_ProjectileSpeed),
            Fallback(enemy.Global.Shooting_FireSpreadClockwise, fallback.shooting_FireSpreadClockwise),
            Fallback(enemy.Global.Shooting_FireSpreadStartAngle, fallback.shooting_FireSpreadStartAngle)
        );

    public override object[] GetStartArgs() => [];

    public override StateType GetStateType() => StateType.Shooting;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, _aiData.Sync_TargetPosX, _aiData.Sync_TargetPosY, _aiData.Intern_Dir);
}
