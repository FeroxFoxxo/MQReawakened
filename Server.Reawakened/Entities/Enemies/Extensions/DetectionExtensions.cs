using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Entities.Colliders;
using Microsoft.Extensions.Logging;

namespace Server.Reawakened.Entities.Enemies.Extensions;

public static class DetectionExtensions
{
    public static bool TryGetDetectionCollider(this BehaviorEnemy enemy, Microsoft.Extensions.Logging.ILogger logger, out EnemyCollider collider)
    {
        collider = null;

        if (enemy == null)
            return false;

        var global = enemy.Global;
        var ai = enemy.AiData;
        var hit = enemy.Hitbox;

        if (global == null || ai == null || hit == null)
            return false;

        var isLookingLeft = ai.Intern_Dir < 0;

        logger.LogDebug("Enemy {EnemyId} is looking {Direction} with detection ranges Front: {FrontRangeX}, Back: {BackRangeX}, Down: {DownRangeY} Up: {UpRangeY}",
            enemy.Id,
            isLookingLeft ? "Left" : "Right",
            global.Global_FrontDetectionRangeX,
            global.Global_BackDetectionRangeX,
            global.Global_FrontDetectionRangeDownY,
            global.Global_FrontDetectionRangeUpY
        );

        var rect = new RectModel(
            hit.BoundingBox.X - (isLookingLeft ? global.Global_FrontDetectionRangeX : global.Global_BackDetectionRangeX),
            hit.BoundingBox.Y - global.Global_FrontDetectionRangeDownY,
            hit.BoundingBox.Width + global.Global_FrontDetectionRangeX + global.Global_BackDetectionRangeX,
            hit.BoundingBox.Height + global.Global_FrontDetectionRangeDownY + global.Global_FrontDetectionRangeDownY
        );

        collider = new EnemyCollider(enemy, rect, true);

        return true;
    }
}
