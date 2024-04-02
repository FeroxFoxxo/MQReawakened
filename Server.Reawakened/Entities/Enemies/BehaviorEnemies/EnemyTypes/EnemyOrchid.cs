using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyOrchid(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void HandleShooting()
    {
        base.HandleShooting();

        if (!AiBehavior.Update(ref AiData, Room.Time))
            ChangeBehavior(StateTypes.LookAround, AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir);
    }

    public override void HandleLookAround()
    {
        base.HandleLookAround();

        DetectPlayers(StateTypes.Shooting);

        if (Room.Time >= BehaviorEndTime)
            ChangeBehavior(StateTypes.Patrol, Position.x, Position.y, AiData.Intern_Dir);
    }
}
