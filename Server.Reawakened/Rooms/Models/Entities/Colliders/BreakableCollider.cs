using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class BreakableCollider(string breakableId, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(breakableId, position, sizeX, sizeY, plane, room, ColliderClass.Breakable)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(breakableId))
                breakable.Damage(attack.Damage, attack.DamageType, attack.Owner);

        if (received is PlayerCollider)
            foreach (var collapsingPlatform in Room.GetEntitiesFromId<CollapsingPlatformComp>(breakableId))
                collapsingPlatform.Collapse(false);
    }
}
