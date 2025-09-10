using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class TriggerReceiverCollider(TriggerReceiverComp triggerReceiverComp) : BaseCollider
{
    public override Vector3Model Position => triggerReceiverComp.Position;
    public override Room Room => triggerReceiverComp.Room;
    public override string Id => triggerReceiverComp.Id;
    public override RectModel BoundingBox => triggerReceiverComp.Rectangle;
    public override string Plane => triggerReceiverComp.ParentPlane;
    public override ColliderType Type => ColliderType.TriggerReceiver;
}
