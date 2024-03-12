using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class EnemyCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderClass.Enemy)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
        {
            if (Room.Enemies.TryGetValue(Id, out var enemy))
            {
                if (Room.IsObjectKilled(Id))
                    return;

                var damage = Room.GetEntityFromId<IDamageable>(Id);

                if (damage != null)
                {
                    var amountToDamage = damage.GetDamageAmount(attack.Damage, attack.DamageType);

                    enemy.Damage(amountToDamage, attack.Owner);
                }
            }
        }
    }
}
