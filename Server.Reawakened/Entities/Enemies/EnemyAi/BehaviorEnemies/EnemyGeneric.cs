using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.EnemyAI.BehaviorEnemies;
public class EnemyGeneric(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
}
