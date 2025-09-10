using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class TCCollider(string id, Vector3Model position, RectModel box, string plane, Room room) : BaseCollider
{
    public override Room Room => room;
    public override string Id => id;
    public override Vector3Model Position => position;
    public override RectModel BoundingBox => box;
    public override string Plane => plane;
    public override ColliderType Type => ColliderType.TerrainCube;
}
