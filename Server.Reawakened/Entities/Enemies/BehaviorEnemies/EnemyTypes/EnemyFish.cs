using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyFish(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    // Uses Position.X/Y instead of AiData.Sync_TargetPosX/Y
    public override void HandleAggro()
    {
        if (!AiBehavior.Update(ref AiData, Room.Time))
            ChangeBehavior(StateTypes.LookAround, Position.x, Position.y, AiData.Intern_Dir);
    }

    // Uses COMEBACK instead of PATROL
    public override void HandleLookAround()
    {
        DetectPlayers(OffensiveBehavior);

        if (Room.Time >= BehaviorEndTime)
        {
            ChangeBehavior(StateTypes.ComeBack, Position.x, AiData.Intern_SpawnPosY, AiData.Intern_Dir);
            AiBehavior.MustDoComeback(AiData);
        }
    }
}
