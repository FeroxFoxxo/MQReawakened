using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class DefaultCollider(string id, Vector3Model position,
    float sizeX, float sizeY, string plane, Room room) :
    BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderType.Default)
{
}
