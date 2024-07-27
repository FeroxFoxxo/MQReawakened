using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class MovingPlatformCollider(string id, Vector3 position, Rect box, string plane, Room room) :
    BaseCollider(id, position, box, plane, room, ColliderType.MovingPlatform)
{
}
