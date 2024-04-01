using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok;
public class EnemySpiderBoss(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : AIStateEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void Initialize()
    {
        base.Initialize();
    }
}
