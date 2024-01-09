using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities;

public class BaseCollider
{
    public int Id;

    public Room Room;
    public Vector3 Position;
    public string Plane;

    public bool IsAttackBox;
    public RectModel ColliderBox;

    public BaseCollider(int id, Vector3Model position, float sizeX, float sizeY, string plane, Room room, bool isAttackBox)
    {
        // Builder for projectiles
        Id = id;
        Position = new Vector3(position.X, position.Y, position.Z);
        Plane = plane;
        IsAttackBox = isAttackBox;
        ColliderBox = new RectModel(position.X - position.X * 0.5f, position.Y - position.Y * 0.5f, sizeX, sizeY);
        Room = room;
    }

    public BaseCollider(int id, Vector3Model position, float sizeX, float sizeY, string plane, Room room)
    {
        // Builder for objects
        Id = id;
        Position = new Vector3(position.X, position.Y, position.Z);
        Plane = plane;
        IsAttackBox = false;
        ColliderBox = new RectModel(position.X, position.Y, sizeX, sizeY);
        Room = room;
    }

    public virtual void Update() { }

    public virtual void OnCollision(BaseCollider collider, SyncEvent syncEvent) { }

    public int[] IsColliding()
    {
        var roomList = Room.Colliders.Values.ToList();
        var collidedWith = new List<int>();

        foreach (var collider in roomList)
        {
            if (CheckCollision(collider) && !collider.IsAttackBox)
                collidedWith.Add(collider.Id);
        }

        return [.. collidedWith];
    }

    public bool CheckCollision(BaseCollider collided) =>
        Position.x < collided.ColliderBox.X + collided.ColliderBox.Width && collided.ColliderBox.X < Position.x + ColliderBox.Width &&
        Position.y < collided.ColliderBox.Y + collided.ColliderBox.Height && collided.ColliderBox.Y < Position.y + ColliderBox.Height &&
        Plane == collided.Plane;
}
