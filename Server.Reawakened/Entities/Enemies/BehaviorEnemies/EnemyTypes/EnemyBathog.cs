using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyBathog(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    // Uses Position.X/Y instead of AiData.Sync_TargetPosX/Y
    public override void HandleAggro()
    {
        if (!AiBehavior.Update(ref AiData, Room.Time))
            ChangeBehavior(GenericScript.AwareBehavior, Position.x, Position.y, AiData.Intern_Dir);
    }

    // Uses AiData.Intern_SpawnPosY instead of Position.Y
    public override void HandleLookAround()
    {
        DetectPlayers(GenericScript.AttackBehavior);

        if (Room.Time >= BehaviorEndTime)
        {
            ChangeBehavior(GenericScript.UnawareBehavior, Position.x, AiData.Intern_SpawnPosY, AiData.Intern_Dir);
            AiBehavior.MustDoComeback(AiData);
        }
    }
}
