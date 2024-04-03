using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyGrenadier(EnemyData data) : BehaviorEnemy(data)
{
    // Use Position.y target rather than player.TempData.Position.Y
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
