using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class TriggerableTargetCollider(string gameObjectId, Vector3Model position, Vector2 size, string plane, Room room) :
    BaseCollider(gameObjectId, position, size, plane, room, ColliderType.TerrainCube)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attackCollider)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerCoopControllerComp>(Id))
                trigger.SendTriggerEvent(attackCollider.Owner);
    }
}
