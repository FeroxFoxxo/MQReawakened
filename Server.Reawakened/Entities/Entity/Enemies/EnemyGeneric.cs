using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyGeneric(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : Enemy(room, entityId, prefabName, enemyController, services)
{
}
