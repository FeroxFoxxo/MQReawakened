using Server.Reawakened.Entities.Components.GameObjects.Controllers;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.Entities.Enemies.Models;
public class EnemyData(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, EnemyModel enemyModel, IServiceProvider services)
{
    public Room Room => room;
    public string EntityId => entityId;
    public string PrefabName => prefabName;
    public EnemyControllerComp EnemyController => enemyController;
    public IServiceProvider Services => services;
    public EnemyModel EnemyModel => enemyModel;
}
