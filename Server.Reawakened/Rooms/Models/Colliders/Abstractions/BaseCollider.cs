using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
public abstract class BaseCollider
{
    public Room Room;
    public Vector3 Position;
    public string Id;
    public string Plane;
    public ColliderType Type;
    public RectModel ColliderBox;
    public Vector3 SpawnPosition;

    public BaseCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room, ColliderType colliderType)
    {
        // Builder for projectiles
        Id = id;
        Position = new Vector3(position.X, position.Y, position.Z);
        SpawnPosition = new Vector3(position.X, position.Y, position.Z);
        Plane = plane;
        Room = room;

        Type = colliderType;
        ColliderBox = new RectModel(position.X, position.Y, sizeX, sizeY);
    }

    public BaseCollider(ColliderModel collider, Room room)
    {
        Id = string.Empty;
        Position = new Vector3(collider.Position.x, collider.Position.y, 0);
        Plane = collider.Plane;
        Room = room;
        Type = ColliderType.TerrainCube;
        ColliderBox = new RectModel(Position.x, Position.y + 0.1f, collider.Width, collider.Height);
    }

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
