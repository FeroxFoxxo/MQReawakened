using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders.Abstractions;
public abstract class BaseCollider(string id, Vector3 pos, Vector2 size, string plane, Room room, ColliderType colliderType)
{
    public Room Room => room;
    public string Id => id;
    public string Plane => plane;
    public ColliderType Type => colliderType;

    public Vector3 Position = new(pos.x, pos.y, pos.z);
    public Vector3 SpawnPosition = new(pos.x, pos.y, pos.z);

    public RectModel ColliderBox = new(pos.x, pos.y, size.x, size.y);

    public virtual string[] IsColliding(bool isAttack) => [];

    public static Vector3 AdjustPosition(Vector3 originalPosition, Vector2 size) => new()
    {
        x = originalPosition.x + Math.Abs(size.x),
        y = originalPosition.y + Math.Abs(size.y),
        z = originalPosition.z
    };

    public virtual void SendCollisionEvent(BaseCollider received)
    {
    }

    public virtual void SendNonCollisionEvent(BaseCollider received)
    {
    }

    public virtual bool CheckCollision(BaseCollider collided) =>
        Position.x < collided.Position.x + collided.ColliderBox.Width && collided.Position.x < Position.x + ColliderBox.Width &&
        Position.y < collided.Position.y + collided.ColliderBox.Height && collided.Position.y < Position.y + ColliderBox.Height &&
        Plane == collided.Plane;
}
