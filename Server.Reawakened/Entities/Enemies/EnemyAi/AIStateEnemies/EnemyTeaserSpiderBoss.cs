using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.EnemyAI.AIStateEnemies;
public class EnemyTeaserSpiderBoss(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : AIStateEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void Initialize()
    {
        base.Initialize();

        //This is here for now, but will be removed when state position syncing is added
        Hitbox.Position.y -= 21;
    }
}
