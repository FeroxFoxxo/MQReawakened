using Server.Reawakened.Rooms.Models.Planes;
using SmartFoxClientAPI.Data;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities;
public abstract class BaseCollider
{
    public Room Room;
    public Vector3 Position;
    public int Id;
    public string Plane;
    public string ColliderType;
    public RectModel ColliderBox;

    public BaseCollider(int id, Vector3Model position, float sizeX, float sizeY, string plane, Room room, string colliderType)
    {
        // Builder for projectiles
        Id = id;
        Position = new Vector3(position.X, position.Y, position.Z);
        Plane = plane;
        Room = room;

        ColliderType = colliderType.ToLower();
        switch (ColliderType)
        {
            case "projectile":
                ColliderBox = new RectModel(position.X - (position.X * 0.5f), position.Y - (position.Y * 0.5f), sizeX, sizeY);
                break;
            default:
                ColliderBox = new RectModel(position.X, position.Y, sizeX, sizeY);
                break;
        }
    }
    public BaseCollider(Collider collider, Room room)
    {
        Id = 0;
        Position = new Vector3(collider.Position.x, collider.Position.y, 0);
        Plane = collider.Plane;
        Room = room;
        ColliderType = "terrain";
        ColliderBox = new RectModel(Position.x, Position.y + 0.1f, collider.Width, collider.Height);
    }

    public virtual int[] IsColliding(bool isAttack)
    {
        return [];
    }

    public virtual void SendCollisionEvent(BaseCollider received)
    {
    }

    public virtual bool CheckCollision(BaseCollider collided)
    {
        if (Position.x < collided.Position.x + collided.ColliderBox.Width && collided.Position.x < Position.x + ColliderBox.Width &&
            Position.y < collided.Position.y + collided.ColliderBox.Height && collided.Position.y < Position.y + ColliderBox.Height &&
            Plane == collided.Plane)
            return true;
        else
            return false;
    }
}
