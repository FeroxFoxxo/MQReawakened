using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Collisions(BehaviorEnemy enemy) : ICollisions
{
    private List<BaseCollider> colliders = [];

    public override void enable(bool enable)
    {
        if (enable)
        {
            foreach (var collider in colliders)
            {
                enemy.Room.AddColliderToList(collider);
            }
        }
        else
        {
            colliders = enemy.Room.GetCollidersById(enemy.Id);
            enemy.Room.RemoveCollider(enemy.Id);
        }
    }
}
