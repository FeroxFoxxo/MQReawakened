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
    public int NbBulletsPerRound => nbBulletsPerRound;
    public float FireSpreadAngle => fireSpreadAngle;
    public float DelayBetweenBullet => delayBetweenBullet;
    public float DelayShootAnim => delayShootAnim;
    public int NbFireRounds => nbFireRounds;
    public float DelayBetweenFireRound => delayBetweenFireRound;
    public float StartCoolDownTime => startCoolDownTime;
    public float EndCoolDownTime => endCoolDownTime;
    public float ProjectileSpeed => projectileSpeed;
    public bool FireSpreadClockwise => fireSpreadClockwise;
    public float FireSpreadStartAngle => fireSpreadStartAngle;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorShooting(this, globalComp);
}
