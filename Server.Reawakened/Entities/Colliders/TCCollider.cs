using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class TCCollider(string id, Vector3 position, Vector2 size, string plane, Room room) :
    BaseCollider(id, position, size, plane, room, ColliderType.TerrainCube)
{
}
