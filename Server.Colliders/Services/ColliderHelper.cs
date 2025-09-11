using Server.Colliders.DTOs;
using Server.Reawakened.Rooms;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Colliders.Abstractions;

namespace Server.Colliders.Services;

public static class ColliderHelper
{
    public static ColliderDto[] BuildCollidersForRoom(Room room)
    {
        var colliders = room.GetColliders().Select(ColliderToDto).ToList();

        try
        {
            foreach (var enemy in room.GetEnemies())
            {
                if (enemy is BehaviorEnemy behavior)
                {
                    if (!behavior.TryGetDetectionCollider(out var detect))
                        continue;

                    colliders.Add(ColliderToDto(detect));
                }
            }
        }
        catch { }

        return [.. colliders];
    }

    private static ColliderDto ColliderToDto(BaseCollider c)
    {
        var type = c.Type.ToString();
        
        return new(
            $"{c.Id}_{type}",
            type,
            c.Plane,
            c.Active,
            c.IsInvisible,
            c.ColliderBox.x,
            c.ColliderBox.y,
            c.ColliderBox.width,
            c.ColliderBox.height
        );
    }
}
