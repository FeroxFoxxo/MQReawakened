using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders.Abstractions;
public abstract class BaseCollider(string id, Vector3Model position, Vector2 size, string plane, Room room, ColliderType colliderType)
{
    public Room Room => room;
    public string Id => id;
    public string Plane => plane;
    public ColliderType Type => colliderType;

    public Vector3 Position = new(position.X, position.Y, position.Z);
    public Vector3 SpawnPosition = new(position.X, position.Y, position.Z);
    public RectModel ColliderBox = new(position.X, position.Y, size.x, size.y);

    public virtual string[] IsColliding(bool isAttack) => [];

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
