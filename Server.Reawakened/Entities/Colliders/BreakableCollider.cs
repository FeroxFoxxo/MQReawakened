using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Platforms;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class BreakableCollider(string breakableId, Vector3 position, Rect box, string plane, Room room, bool enemyHurt) :
    BaseCollider(breakableId, position, box, plane, room, ColliderType.Breakable)
{
    public bool EnemyHurt = enemyHurt;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack && !EnemyHurt)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(Id))
                breakable.Damage(attack.Damage, attack.DamageType, attack.Owner);

        if (received is AIProjectileCollider aiprj && EnemyHurt)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(Id))
                breakable.Damage(aiprj.Damage, aiprj.OwnderId);

        if (received is PlayerCollider)
            foreach (var collapsingPlatform in Room.GetEntitiesFromId<CollapsingPlatformComp>(Id))
                collapsingPlatform.Collapse(false);
    }
}
