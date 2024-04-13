using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorShooting(ShootingProperties shootingState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public int NbBulletsPerRound => Enemy.Global.Shooting_NbBulletsPerRound != Enemy.Global.Default.Shooting_NbBulletsPerRound ? Enemy.Global.Shooting_NbBulletsPerRound : shootingState.shooting_NbBulletsPerRound;
    public float FireSpreadAngle => Enemy.Global.Shooting_FireSpreadAngle != Enemy.Global.Default.Shooting_FireSpreadAngle ? Enemy.Global.Shooting_FireSpreadAngle : shootingState.shooting_FireSpreadAngle;
    public float DelayBetweenBullet => Enemy.Global.Shooting_DelayBetweenBullet != Enemy.Global.Default.Shooting_DelayBetweenBullet ? Enemy.Global.Shooting_DelayBetweenBullet : shootingState.shooting_DelayBetweenBullet;
    public float DelayShootAnim => Enemy.Global.Shooting_DelayShot_Anim != Enemy.Global.Default.Shooting_DelayShot_Anim ? Enemy.Global.Shooting_DelayShot_Anim : shootingState.shooting_DelayShoot_Anim;
    public int NbFireRounds => Enemy.Global.Shooting_NbFireRounds != Enemy.Global.Default.Shooting_NbFireRounds ? Enemy.Global.Shooting_NbFireRounds : shootingState.shooting_NbFireRounds;
    public float DelayBetweenFireRound => Enemy.Global.Shooting_DelayBetweenFireRound != Enemy.Global.Default.Shooting_DelayBetweenFireRound ? Enemy.Global.Shooting_DelayBetweenFireRound : shootingState.shooting_DelayBetweenFireRound;
    public float StartCoolDownTime => Enemy.Global.Shooting_StartCoolDownTime != Enemy.Global.Default.Shooting_StartCoolDownTime ? Enemy.Global.Shooting_StartCoolDownTime : shootingState.shooting_StartCoolDownTime;
    public float EndCoolDownTime => Enemy.Global.Shooting_EndCoolDownTime != Enemy.Global.Default.Shooting_EndCoolDownTime ? Enemy.Global.Shooting_EndCoolDownTime : shootingState.shooting_EndCoolDownTime;
    public float ProjectileSpeed => Enemy.Global.Shooting_ProjectileSpeed != Enemy.Global.Default.Shooting_ProjectileSpeed ? Enemy.Global.Shooting_ProjectileSpeed : shootingState.shooting_ProjectileSpeed;
    public bool FireSpreadClockwise => Enemy.Global.Shooting_FireSpreadClockwise != Enemy.Global.Default.Shooting_FireSpreadClockwise ? Enemy.Global.Shooting_FireSpreadClockwise : shootingState.shooting_FireSpreadClockwise;
    public float FireSpreadStartAngle => Enemy.Global.Shooting_FireSpreadStartAngle != Enemy.Global.Default.Shooting_FireSpreadStartAngle ? Enemy.Global.Shooting_FireSpreadStartAngle : shootingState.shooting_FireSpreadStartAngle;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Shooting;

    public override object[] GetProperties() => [
        NbBulletsPerRound, FireSpreadAngle,
        DelayBetweenBullet, DelayShootAnim,
        NbFireRounds, DelayBetweenFireRound,
        StartCoolDownTime, EndCoolDownTime,
        ProjectileSpeed,
        FireSpreadClockwise, FireSpreadStartAngle
    ];

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(Enemy.GenericScript.AwareBehavior, Enemy.AiData.Sync_TargetPosX, Enemy.AiData.Sync_TargetPosY, Enemy.AiData.Intern_Dir);
}
