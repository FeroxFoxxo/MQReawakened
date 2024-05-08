using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class EnemyCollider(string id, Vector3 position, Rect box, string plane, Room room) :
    BaseCollider(id, position, box, plane, room, ColliderType.Enemy)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
        {
            var damage = Room.GetEntityFromId<IDamageable>(Id)
                .GetDamageAmount(attack.Damage, attack.DamageType);

            var enemy = Room.GetEnemy(Id);

            if (enemy != null)
            {
                if (Room.IsObjectKilled(Id))
                    return;

                enemy.Damage(attack.Owner, damage);
            }
        }
    }
}
