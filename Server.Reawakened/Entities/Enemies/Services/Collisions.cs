using Server.Reawakened.Entities.Enemies.EnemyTypes;

namespace Server.Reawakened.Entities.Enemies.Services;
public class Collisions(BehaviorEnemy enemy) : ICollisions
{
    public override void enable(bool enable)
    {
        if (enable)
            enemy.Room.AddCollider(enemy.Hitbox);
        else
            enemy.Room.RemoveCollider(enemy.Hitbox.Id);
    }
}
