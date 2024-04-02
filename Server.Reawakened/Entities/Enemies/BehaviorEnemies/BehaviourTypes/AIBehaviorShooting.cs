using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorShooting(ShootingState shootingState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public int NbBulletsPerRound => globalComp.Shooting_NbBulletsPerRound != default ? globalComp.Shooting_NbBulletsPerRound : shootingState.NbBulletsPerRound;
    public float FireSpreadAngle => globalComp.Shooting_FireSpreadAngle != default ? globalComp.Shooting_FireSpreadAngle : shootingState.FireSpreadAngle;
    public float DelayBetweenBullet => globalComp.Shooting_DelayBetweenBullet != default ? globalComp.Shooting_DelayBetweenBullet : shootingState.DelayBetweenBullet;
    public float DelayShootAnim => globalComp.Shooting_DelayShot_Anim != default ? globalComp.Shooting_DelayShot_Anim : shootingState.DelayShootAnim;
    public int NbFireRounds => globalComp.Shooting_NbFireRounds != default ? globalComp.Shooting_NbFireRounds : shootingState.NbFireRounds;
    public float DelayBetweenFireRound => globalComp.Shooting_DelayBetweenFireRound != default ? globalComp.Shooting_DelayBetweenFireRound : shootingState.DelayBetweenFireRound;
    public float StartCoolDownTime => globalComp.Shooting_StartCoolDownTime != default ? globalComp.Shooting_StartCoolDownTime : shootingState.StartCoolDownTime;
    public float EndCoolDownTime => globalComp.Shooting_EndCoolDownTime != default ? globalComp.Shooting_EndCoolDownTime : shootingState.EndCoolDownTime;
    public float ProjectileSpeed => globalComp.Shooting_ProjectileSpeed != default ? globalComp.Shooting_ProjectileSpeed : shootingState.ProjectileSpeed;
    public bool FireSpreadClockwise => globalComp.Shooting_FireSpreadClockwise != default ? globalComp.Shooting_FireSpreadClockwise : shootingState.FireSpreadClockwise;
    public float FireSpreadStartAngle => globalComp.Shooting_FireSpreadStartAngle != default ? globalComp.Shooting_FireSpreadStartAngle : shootingState.FireSpreadStartAngle;

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Shooting(NbBulletsPerRound, FireSpreadAngle, DelayBetweenBullet, DelayShootAnim,
        NbFireRounds, DelayBetweenFireRound, StartCoolDownTime, EndCoolDownTime, ProjectileSpeed, FireSpreadClockwise, FireSpreadStartAngle);

    public override StateTypes GetBehavior() => StateTypes.Shooting;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(NbBulletsPerRound);
        sb.Append(FireSpreadAngle);
        sb.Append(DelayBetweenBullet);
        sb.Append(DelayShootAnim);
        sb.Append(NbFireRounds);
        sb.Append(DelayBetweenFireRound);
        sb.Append(StartCoolDownTime);
        sb.Append(EndCoolDownTime);
        sb.Append(ProjectileSpeed);
        sb.Append(FireSpreadClockwise ? 1 : 0);
        sb.Append(FireSpreadStartAngle);

        return sb.ToString();
    }
}
