using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class TriggerableTargetCollider(BaseComponent baseTriggerCoop) : BaseCollider
{
    public override Vector3Model Position => baseTriggerCoop.Position;
    public override Room Room => baseTriggerCoop.Room;
    public override string Id => baseTriggerCoop.Id;
    public override RectModel BoundingBox => baseTriggerCoop.Rectangle;
    public override string Plane => baseTriggerCoop.ParentPlane;
    public override ColliderType Type => ColliderType.TriggerTarget;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attackCollider)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerCoopControllerComp>(Id))
                trigger.SendTriggerEvent(attackCollider.Owner);
    }
}
