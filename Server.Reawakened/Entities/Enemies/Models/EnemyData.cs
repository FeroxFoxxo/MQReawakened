using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.Entities.Enemies.Models;
public class EnemyData(Room room, string entityId, string prefabName, IEnemyController enemyController, EnemyModel enemyModel, IServiceProvider services)
{
    public Room Room => room;
    public string EntityId => entityId;
    public string PrefabName => prefabName;
    public IEnemyController EnemyController => enemyController;
    public IServiceProvider Services => services;
    public EnemyModel EnemyModel => enemyModel;
}
