using Server.Reawakened.Entities.Enemies.Models;

namespace Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok;
public class EnemyTeaserSpiderBoss(EnemyData data) : AIStateEnemy(data)
{
    public override void Initialize()
    {
        base.Initialize();

        //This is here for now, but will be removed when state position syncing is added
        Hitbox.Position.y -= 21;
    }
}
