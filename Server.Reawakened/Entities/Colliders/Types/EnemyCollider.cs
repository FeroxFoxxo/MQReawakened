using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Controllers;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class EnemyCollider(string id, Vector3Model position,
    float sizeX, float sizeY, string plane, Room room) :
    BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderType.Enemy)
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

                enemy.Damage(damage, attack.Owner);
            }
            else
            {
                var enemyController = Room.GetEntitiesFromId<EnemyControllerComp>(Id).First();
                enemyController?.Damage(damage, attack.Owner);
            }
        }
    }
}
