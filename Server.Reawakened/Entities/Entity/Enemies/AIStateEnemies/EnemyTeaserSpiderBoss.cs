using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Entity.Enemies.BehaviorEnemies;
public class EnemyTeaserSpiderBoss(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : AIStateEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void Initialize() 
    {
        base.Initialize();

        //This is here for now, but will be removed when state position syncing is added
        Hitbox.Position.y -= 21;
    }
}
