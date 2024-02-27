using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;
using System.Numerics;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class TriggerableTargetCollider(string gameObjectId, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(new ColliderModel(plane, position.X, position.Y, sizeX, sizeY), room)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attackCollider)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerCoopControllerComp>(gameObjectId))
                trigger.SendTriggerEvent(attackCollider.Owner);
    }
}
