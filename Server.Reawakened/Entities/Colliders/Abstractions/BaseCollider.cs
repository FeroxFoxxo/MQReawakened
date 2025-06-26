using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Attributes;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders.Abstractions;
public abstract class BaseCollider
{
    public readonly Room Room;
    public readonly string Id;
    public readonly string Plane;
    public readonly ColliderType Type;
    public readonly Vector3 SpawnPosition;
    public readonly Rect BoundingBox;
    public readonly bool IsInvisible;

    private Vector3 internalPosition = Vector3.zero;
    private Rect colliderBox = new(0, 0, 0, 0);

    public bool Active;
    public Vector3 Position
    {
        get => internalPosition;
        set
        {
            internalPosition = value;
            colliderBox = new Rect(
                internalPosition.x + BoundingBox.x,
                internalPosition.y + BoundingBox.y,
                BoundingBox.width,
                BoundingBox.height
            );
        }
    }

    protected BaseCollider(string id, Vector3 position, Rect boundingBox, string plane, Room room, ColliderType colliderType)
    {
        Room = room;
        Id = id;
        Plane = plane;
        Type = colliderType;
        BoundingBox = boundingBox;
        SpawnPosition = new Vector3(position.x, position.y, position.z);
        Active = true;

        var invis = Room.GetEntityFromId<InvisibilityControllerComp>(Id);
        IsInvisible = invis != null && invis.ApplyInvisibility;

        // MUST be at bottom so collider generates correctly.
        Position = new Vector3(position.x, position.y, position.z);
    }

    public virtual string[] IsColliding() => [];

    public virtual string[] IsColliding(bool isAttack) => [];

    public virtual void SendCollisionEvent(BaseCollider received)
    {
    }

    public virtual void SendNonCollisionEvent(BaseCollider received)
    {
    }

    private static bool RectOverlapsRect(Rect rA, Rect rB) =>
        rA.x < rB.x + rB.width && rA.x + rA.width > rB.x && rA.y < rB.y + rB.height && rA.y + rA.height > rB.y;
    
    public bool CheckCollision(BaseCollider collided) =>
        RectOverlapsRect(collided.colliderBox, colliderBox) && Plane == collided.Plane;
}
