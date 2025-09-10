using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class EnemyCollider(BaseEnemy enemy, Vector3 position, Rect box, string plane, Room room, bool isDetection = false) :
    BaseCollider(enemy.Id, position, box, plane, room, ColliderType.Enemy, !isDetection)
{
    public BaseEnemy Enemy => enemy;

    protected override Vector3 InternalPosition => Enemy.Position.ToUnityVector3();

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
        {
            var damage = Room.GetEntityFromId<IDamageable>(Enemy.Id)
                .GetDamageAmount(attack.Damage, attack.DamageType);

            if (Room.IsObjectKilled(Enemy.Id))
                return;

            Enemy.Damage(attack.Owner, damage);
        }
    }
}
