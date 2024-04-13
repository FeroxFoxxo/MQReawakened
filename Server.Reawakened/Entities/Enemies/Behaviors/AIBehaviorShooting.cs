using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorShooting(ShootingProperties properties, BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => new ShootingProperties(
        Enemy.Global.Shooting_NbBulletsPerRound != Enemy.Global.Default.Shooting_NbBulletsPerRound ? Enemy.Global.Shooting_NbBulletsPerRound : properties.shooting_NbBulletsPerRound,
        Enemy.Global.Shooting_FireSpreadAngle != Enemy.Global.Default.Shooting_FireSpreadAngle ? Enemy.Global.Shooting_FireSpreadAngle : properties.shooting_FireSpreadAngle,
        Enemy.Global.Shooting_DelayBetweenBullet != Enemy.Global.Default.Shooting_DelayBetweenBullet ? Enemy.Global.Shooting_DelayBetweenBullet : properties.shooting_DelayBetweenBullet,
        Enemy.Global.Shooting_DelayShot_Anim != Enemy.Global.Default.Shooting_DelayShot_Anim ? Enemy.Global.Shooting_DelayShot_Anim : properties.shooting_DelayShoot_Anim,
        Enemy.Global.Shooting_NbFireRounds != Enemy.Global.Default.Shooting_NbFireRounds ? Enemy.Global.Shooting_NbFireRounds : properties.shooting_NbFireRounds,
        Enemy.Global.Shooting_DelayBetweenFireRound != Enemy.Global.Default.Shooting_DelayBetweenFireRound ? Enemy.Global.Shooting_DelayBetweenFireRound : properties.shooting_DelayBetweenFireRound,
        Enemy.Global.Shooting_StartCoolDownTime != Enemy.Global.Default.Shooting_StartCoolDownTime ? Enemy.Global.Shooting_StartCoolDownTime : properties.shooting_StartCoolDownTime,
        Enemy.Global.Shooting_EndCoolDownTime != Enemy.Global.Default.Shooting_EndCoolDownTime ? Enemy.Global.Shooting_EndCoolDownTime : properties.shooting_EndCoolDownTime,
        Enemy.Global.Shooting_ProjectileSpeed != Enemy.Global.Default.Shooting_ProjectileSpeed ? Enemy.Global.Shooting_ProjectileSpeed : properties.shooting_ProjectileSpeed,
        Enemy.Global.Shooting_FireSpreadClockwise != Enemy.Global.Default.Shooting_FireSpreadClockwise ? Enemy.Global.Shooting_FireSpreadClockwise : properties.shooting_FireSpreadClockwise,
        Enemy.Global.Shooting_FireSpreadStartAngle != Enemy.Global.Default.Shooting_FireSpreadStartAngle ? Enemy.Global.Shooting_FireSpreadStartAngle : properties.shooting_FireSpreadStartAngle
    );

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(Enemy.GenericScript.AwareBehavior, Enemy.AiData.Sync_TargetPosX, Enemy.AiData.Sync_TargetPosY, Enemy.AiData.Intern_Dir);
}
