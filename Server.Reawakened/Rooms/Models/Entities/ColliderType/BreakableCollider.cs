using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class BreakableCollider(string breakableId, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, position, sizeX, sizeY, plane, room, "breakable")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(breakableId))
                breakable.Damage(attack.Damage, attack.Owner);
    }
}
