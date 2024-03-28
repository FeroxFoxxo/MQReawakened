using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class EnemyCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderClass.Enemy)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
        {
            Room.Enemies.TryGetValue(Id, out var enemy);

            var damage = Room.GetEntityFromId<IDamageable>(id)
                .GetDamageAmount(attack.Damage, attack.DamageType);

            if (enemy != null)
            {
                if (Room.IsObjectKilled(Id))
                    return;

                enemy.Damage(damage, attack.Owner);
            }

            else
            {
                var enemyController = Room.GetEntitiesFromId<EnemyControllerComp>(id).First();
                enemyController?.Damage(damage, attack.Owner);
            }
        }
    }
}
