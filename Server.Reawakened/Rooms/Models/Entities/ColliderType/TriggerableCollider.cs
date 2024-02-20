using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;
using System.Numerics;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class TriggerableTargetCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, new ColliderModel(plane, position.X, position.Y, sizeX, sizeY), room)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attackCollider)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerCoopControllerComp>(Id))
                trigger.SendTriggerEvent(attackCollider.Owner);
    }
}
