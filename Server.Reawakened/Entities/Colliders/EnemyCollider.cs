using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;
public class EnemyCollider(BaseEnemy enemy, RectModel box) : BaseCollider
{
    public override Vector3Model Position => enemy.Position;
    public override Room Room => enemy.Room;
    public override string Id => enemy.Id;
    public override RectModel BoundingBox => box;
    public override string Plane => enemy.ParentPlane;
    public override ColliderType Type => ColliderType.Enemy;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
        {
            if (Room.IsObjectKilled(enemy.Id))
                return;

            var damage = Room.GetEntityFromId<IDamageable>(enemy.Id)
                .GetDamageAmount(attack.Damage, attack.DamageType);

            enemy.Damage(attack.Owner, damage);
        }
        else if (received is PlayerCollider playerCollider)
        {
            if (Room.IsObjectKilled(enemy.Id) || playerCollider.Player.TempData.Invincible)
                return;

            enemy.OnCollideWithPlayer(playerCollider.Player);
        }
    }
}
