using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
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

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorShooting(this, globalComp);
}
