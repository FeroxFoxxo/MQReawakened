using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class TriggerableTargetCollider(string gameObjectId, Vector3 position, Rect box, string plane, Room room) :
    BaseCollider(gameObjectId, position, box, plane, room, ColliderType.TerrainCube)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attackCollider)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerCoopControllerComp>(Id))
                trigger.SendTriggerEvent(attackCollider.Owner);
    }
}
