using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Platforms;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class BreakableCollider(string breakableId, Vector3Model position,
    float sizeX, float sizeY, string plane, Room room) :
    BaseCollider(breakableId, position, sizeX, sizeY, plane, room, ColliderType.Breakable)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(Id))
                breakable.Damage(attack.Damage, attack.DamageType, attack.Owner);

        if (received is PlayerCollider)
            foreach (var collapsingPlatform in Room.GetEntitiesFromId<CollapsingPlatformComp>(Id))
                collapsingPlatform.Collapse(false);
    }
}
