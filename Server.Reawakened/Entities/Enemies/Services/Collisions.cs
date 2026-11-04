using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Collisions(BehaviorEnemy enemy) : ICollisions
{
    private List<BaseCollider> colliders = [];

    public override void enable(bool enable)
    {
        if (colliders.Count <= 0)
            colliders = enemy.Room.GetCollidersById(enemy.Id);

        if (enable)
        {
            foreach (var collider in colliders)
            {
                enemy.Logger.LogInformation("Collisions Enabled");
                enemy.Room.ToggleCollider(collider.Id, true);
            }
        }
        else
        {
            foreach (var collider in colliders)
            {
                enemy.Logger.LogInformation("Collisions Disabled");
                enemy.Room.ToggleCollider(collider.Id, false);
            }
        }
    }
}
