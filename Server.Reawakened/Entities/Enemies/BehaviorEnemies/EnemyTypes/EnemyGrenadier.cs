using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyGrenadier(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{    
    public override void HandleGrenadier()
    {
        base.HandleGrenadier();

        if (Room.Time >= BehaviorEndTime)
            ChangeBehavior(StateTypes.LookAround, Position.x, Position.y, AiData.Intern_Dir);
    }

    public override void HandleLookAround()
    {
        base.HandleLookAround();

        DetectPlayers(StateTypes.Grenadier);

        if (Room.Time >= BehaviorEndTime)
            ChangeBehavior(StateTypes.Patrol, Position.x, Position.y, Generic.Patrol_ForceDirectionX);
    }

    public override void DetectPlayers(StateTypes behaviorToRun)
    {
        foreach (var player in Room.Players.Values)
        {
            if (PlayerInRange(player.TempData.Position, GlobalProperties.Global_DetectionLimitedByPatrolLine))
            {
                AiData.Sync_TargetPosX = player.TempData.Position.X;
                AiData.Sync_TargetPosY = Position.y;

                ChangeBehavior(behaviorToRun, player.TempData.Position.X, player.TempData.Position.Y, Generic.Patrol_ForceDirectionX);
            }
        }
    }
}
