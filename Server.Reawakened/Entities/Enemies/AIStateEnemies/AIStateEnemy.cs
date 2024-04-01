using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.AIStateEnemies;

public abstract class AIStateEnemy(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : Enemy(room, entityId, prefabName, enemyController, services), IDestructible
{
}
