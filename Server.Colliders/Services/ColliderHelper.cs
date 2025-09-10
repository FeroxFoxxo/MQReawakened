using Server.Colliders.DTOs;
using Server.Reawakened.Rooms;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Colliders.Enums;

namespace Server.Colliders.Services;

public static class ColliderHelper
{
    public static ColliderDto[] BuildCollidersForRoom(Room room)
    {
        var colliders = room.GetColliders().Select(c => new ColliderDto(
            c.Id,
            c.Type.ToString(),
            c.Plane,
            c.Active,
            c.IsInvisible,
            c.ColliderBox.x,
            c.ColliderBox.y,
            c.ColliderBox.width,
            c.ColliderBox.height)).ToList();

        try
        {
            foreach (var enemy in room.GetEnemies())
            {
                if (enemy is BehaviorEnemy behavior)
                {
                    if (!behavior.TryGetDetectionCollider(room.Logger, out var detect))
                        continue;

                    colliders.Add(new ColliderDto(
                        behavior.Id + "_detection",
                        ColliderType.Enemy.ToString(),
                        behavior.ParentPlane,
                        true,
                        detect.IsInvisible,
                        detect.ColliderBox.x,
                        detect.ColliderBox.y,
                        detect.ColliderBox.width,
                        detect.ColliderBox.height));
                }
            }
        }
        catch { }

        return [.. colliders];
    }
}
