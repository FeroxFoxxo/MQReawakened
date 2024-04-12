using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
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

    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorShooting(this, enemy);
}
