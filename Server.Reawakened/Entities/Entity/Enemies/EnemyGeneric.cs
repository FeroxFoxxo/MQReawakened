using Microsoft.Extensions.Logging;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyGeneric(Room room, string entityId, BaseComponent baseEntity, IServiceProvider services) : Enemy(room, entityId, baseEntity, services)
{
}
