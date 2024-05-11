using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Runnable(BehaviorEnemy enemy) : IRunnable
{
    public override void run() => enemy.Damage(null, enemy.Health);
}
