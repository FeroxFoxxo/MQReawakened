using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyVespid(EnemyData data) : BehaviorEnemy(data)
{
    public override void Initialize()
    {
        Position.z = 10;

        base.Initialize();
        
        AiData.Intern_Dir = Generic.Patrol_ForceDirectionX;
    }

    // Use current position for Y target rather than player.TempData.Position.Y
    public override void DetectPlayers(StateTypes behaviorToRun)
    {
        foreach (var player in Room.Players)
        {
            if (PlayerInRange(player.Value.TempData.Position, GlobalProperties.Global_DetectionLimitedByPatrolLine))
            {
                AiData.Sync_TargetPosX = player.Value.TempData.Position.X;
                AiData.Sync_TargetPosY = Position.y;

                ChangeBehavior(behaviorToRun, player.Value.TempData.Position.X, Position.y, Generic.Patrol_ForceDirectionX);
            }
        }
    }
}
