using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class ShootingState(int nbBulletsPerRound, float fireSpreadAngle, float delayBetweenBullet,
    float delayShootAnim, int nbFireRounds, float delayBetweenFireRound, float startCoolDownTime,
    float endCoolDownTime, float projectileSpeed, bool fireSpreadClockwise, float fireSpreadStartAngle,
    List<EnemyResourceModel> resources) : BaseState(resources)
{
    public int NbBulletsPerRound { get; } = nbBulletsPerRound;
    public float FireSpreadAngle { get; } = fireSpreadAngle;
    public float DelayBetweenBullet { get; } = delayBetweenBullet;
    public float DelayShootAnim { get; } = delayShootAnim;
    public int NbFireRounds { get; } = nbFireRounds;
    public float DelayBetweenFireRound { get; } = delayBetweenFireRound;
    public float StartCoolDownTime { get; } = startCoolDownTime;
    public float EndCoolDownTime { get; } = endCoolDownTime;
    public float ProjectileSpeed { get; } = projectileSpeed;
    public bool FireSpreadClockwise { get; } = fireSpreadClockwise;
    public float FireSpreadStartAngle { get; } = fireSpreadStartAngle;

    public override AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic) => new AIBehaviorShooting(this);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) => [];

    public override string ToStateString(AIStatsGenericComp generic)
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
