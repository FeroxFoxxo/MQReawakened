using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyOrchid(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    // Uses Intern_Dir instead of Patrol_ForceDirectionX
    public override void HandleLookAround()
    {
        DetectPlayers(GenericScript.AttackBehavior);

        if (Room.Time >= BehaviorEndTime)
            ChangeBehavior(GenericScript.UnawareBehavior, Position.x, Position.y, AiData.Intern_Dir);
    }
}
