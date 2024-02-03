using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using SmartFoxClientAPI.Data;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities;
public abstract class BaseCollider
{
    public Room Room;
    public Vector3 Position;
    public string Id;
    public string Plane;
    public string ColliderType;
    public RectModel ColliderBox;

    public BaseCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room, string colliderType)
    {
        // Builder for projectiles
        Id = id;
        Position = new Vector3(position.X, position.Y, position.Z);
        Plane = plane;
        Room = room;

        ColliderType = colliderType.ToLower();
        ColliderBox = new RectModel(position.X, position.Y, sizeX, sizeY);
    }
    public BaseCollider(ColliderModel collider, Room room)
    {
        Id = string.Empty;
        Position = new Vector3(collider.Position.x, collider.Position.y, 0);
        Plane = collider.Plane;
        Room = room;
        ColliderType = "terrain";
        ColliderBox = new RectModel(Position.x, Position.y + 0.1f, collider.Width, collider.Height);
    }

    public virtual string[] IsColliding(bool isAttack) => [];

    public virtual void SendCollisionEvent(BaseCollider received)
    {
    }

    public virtual bool CheckCollision(BaseCollider collided) => Position.x < collided.Position.x + collided.ColliderBox.Width && collided.Position.x < Position.x + ColliderBox.Width &&
            Position.y < collided.Position.y + collided.ColliderBox.Height && collided.Position.y < Position.y + ColliderBox.Height &&
            Plane == collided.Plane;

    public bool CheckPlayerCollision(DefaultCollider collided) => collided.ColliderBox.X < ColliderBox.X + ColliderBox.Width && collided.ColliderBox.X > ColliderBox.X - ColliderBox.Width &&
                   collided.ColliderBox.Y > ColliderBox.Y - ColliderBox.Height && collided.ColliderBox.Y < ColliderBox.Y + ColliderBox.Height &&
                   collided.Plane == Plane;
}
