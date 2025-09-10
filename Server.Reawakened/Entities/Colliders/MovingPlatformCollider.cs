using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class MovingPlatformCollider(BaseComponent movingController) : BaseCollider
{
    public override Vector3Model Position => movingController.Position;
    public override Room Room => movingController.Room;
    public override string Id => movingController.Id;
    public override RectModel BoundingBox => movingController.Rectangle;
    public override string Plane => movingController.ParentPlane;
    public override ColliderType Type => ColliderType.MovingPlatform;
}
