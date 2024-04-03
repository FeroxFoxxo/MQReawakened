using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies;
public class EnemyData(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services)
{
    public Room Room => room;
    public string EntityId => entityId;
    public string PrefabName => prefabName;
    public EnemyControllerComp EnemyController => enemyController;
    public IServiceProvider Services => services;
}
