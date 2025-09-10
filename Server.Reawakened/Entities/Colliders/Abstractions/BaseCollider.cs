using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Attributes;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders.Abstractions;
public abstract class BaseCollider
{
    public abstract Room Room { get; }
    public abstract string Id { get; }
    public abstract Vector3Model Position { get; }
    public abstract RectModel BoundingBox { get; }
    public abstract string Plane { get; }
    public abstract ColliderType Type { get; }

    public bool IsInvisible { get; private set; }
    public bool Active { get; set; }
    public Vector3 SpawnPosition { get; private set; }


    private Rect ColliderBox => new(
            Position.X + BoundingBox.X,
            Position.Y + BoundingBox.Y,
            BoundingBox.Width,
            BoundingBox.Height
        );

    protected BaseCollider(bool addToRoom = true)
    {
        SpawnPosition = Position.ToUnityVector3();
        Active = true;

        var invisible = Room.GetEntityFromId<InvisibilityControllerComp>(Id);

        IsInvisible = invisible != null && invisible.ApplyInvisibility;

        if (addToRoom)
            Room.AddColliderToList(this);
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
        RectOverlapsRect(collided.ColliderBox, ColliderBox) && Plane == collided.Plane;
}
