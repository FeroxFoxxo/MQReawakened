using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorShooting(ShootingState shootingState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public int NbBulletsPerRound => globalComp.Shooting_NbBulletsPerRound != globalComp.Default.Shooting_NbBulletsPerRound ? globalComp.Shooting_NbBulletsPerRound : shootingState.NbBulletsPerRound;
    public float FireSpreadAngle => globalComp.Shooting_FireSpreadAngle != globalComp.Default.Shooting_FireSpreadAngle ? globalComp.Shooting_FireSpreadAngle : shootingState.FireSpreadAngle;
    public float DelayBetweenBullet => globalComp.Shooting_DelayBetweenBullet != globalComp.Default.Shooting_DelayBetweenBullet ? globalComp.Shooting_DelayBetweenBullet : shootingState.DelayBetweenBullet;
    public float DelayShootAnim => globalComp.Shooting_DelayShot_Anim != globalComp.Default.Shooting_DelayShot_Anim ? globalComp.Shooting_DelayShot_Anim : shootingState.DelayShootAnim;
    public int NbFireRounds => globalComp.Shooting_NbFireRounds != globalComp.Default.Shooting_NbFireRounds ? globalComp.Shooting_NbFireRounds : shootingState.NbFireRounds;
    public float DelayBetweenFireRound => globalComp.Shooting_DelayBetweenFireRound != globalComp.Default.Shooting_DelayBetweenFireRound ? globalComp.Shooting_DelayBetweenFireRound : shootingState.DelayBetweenFireRound;
    public float StartCoolDownTime => globalComp.Shooting_StartCoolDownTime != globalComp.Default.Shooting_StartCoolDownTime ? globalComp.Shooting_StartCoolDownTime : shootingState.StartCoolDownTime;
    public float EndCoolDownTime => globalComp.Shooting_EndCoolDownTime != globalComp.Default.Shooting_EndCoolDownTime ? globalComp.Shooting_EndCoolDownTime : shootingState.EndCoolDownTime;
    public float ProjectileSpeed => globalComp.Shooting_ProjectileSpeed != globalComp.Default.Shooting_ProjectileSpeed ? globalComp.Shooting_ProjectileSpeed : shootingState.ProjectileSpeed;
    public bool FireSpreadClockwise => globalComp.Shooting_FireSpreadClockwise != globalComp.Default.Shooting_FireSpreadClockwise ? globalComp.Shooting_FireSpreadClockwise : shootingState.FireSpreadClockwise;
    public float FireSpreadStartAngle => globalComp.Shooting_FireSpreadStartAngle != globalComp.Default.Shooting_FireSpreadStartAngle ? globalComp.Shooting_FireSpreadStartAngle : shootingState.FireSpreadStartAngle;

    public override bool ShouldDetectPlayers => false;

    protected override AI_Behavior GetBehaviour() =>
        new AI_Behavior_Shooting(NbBulletsPerRound, FireSpreadAngle, DelayBetweenBullet, DelayShootAnim,
        NbFireRounds, DelayBetweenFireRound, StartCoolDownTime, EndCoolDownTime, ProjectileSpeed, FireSpreadClockwise, FireSpreadStartAngle);

    public override object[] GetData() => [
        NbBulletsPerRound, FireSpreadAngle,
        DelayBetweenBullet, DelayShootAnim,
        NbFireRounds, DelayBetweenFireRound,
        StartCoolDownTime, EndCoolDownTime,
        ProjectileSpeed,
        FireSpreadClockwise, FireSpreadStartAngle
    ];

    public override void NextState(BehaviorEnemy enemy) =>
        enemy.ChangeBehavior(enemy.GenericScript.AwareBehavior, enemy.AiData.Sync_TargetPosX, enemy.AiData.Sync_TargetPosY, enemy.AiData.Intern_Dir);
}
