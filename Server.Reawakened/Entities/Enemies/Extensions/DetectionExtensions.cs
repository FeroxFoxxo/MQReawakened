using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Entities.Colliders;

namespace Server.Reawakened.Entities.Enemies.Extensions;

public static class DetectionExtensions
{
    public static bool TryGetDetectionCollider(this BehaviorEnemy enemy, out EnemyDetectionCollider collider)
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

        var rect = new RectModel(
            hit.BoundingBox.X - (isLookingLeft ? global.Global_FrontDetectionRangeX : global.Global_BackDetectionRangeX),
            hit.BoundingBox.Y - global.Global_FrontDetectionRangeDownY,
            hit.BoundingBox.Width + global.Global_FrontDetectionRangeX + global.Global_BackDetectionRangeX,
            hit.BoundingBox.Height + global.Global_FrontDetectionRangeDownY + global.Global_FrontDetectionRangeDownY
        );

        collider = new EnemyDetectionCollider(enemy, rect);

        return true;
    }
}
