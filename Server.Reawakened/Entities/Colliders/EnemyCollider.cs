using A2m.Server;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
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
            if (enemy == null || Room.IsObjectKilled(enemy.Id))
                return;

            enemy.SendTypeOfDamage(attack.Owner, attack.ItemEffects, attack.PrefabFrom, attack.DamageScale);
        }
        else if (received is PlayerCollider playerCollider)
        {
            if (Room.IsObjectKilled(enemy.Id) || playerCollider.Player.Character.StatusEffects.HasEffect(ItemEffectType.Invincibility))
                return;

            enemy.OnCollideWithPlayer(playerCollider.Player);
        }
    }
}
