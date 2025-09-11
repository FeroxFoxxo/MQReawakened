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

    public Rect ColliderBox => new(
            Position.X + BoundingBox.X,
            Position.Y + BoundingBox.Y,
            BoundingBox.Width,
            BoundingBox.Height
        );

    protected BaseCollider(bool addToRoom = true)
    {
        Active = true;

        var invisible = Room.GetEntityFromId<InvisibilityControllerComp>(Id);

        IsInvisible = invisible != null && invisible.ApplyInvisibility;

        if (addToRoom)
            Room.AddColliderToList(this);
    }

    public virtual string[] RunCollisionDetection() => [];

    public virtual void SendCollisionEvent(BaseCollider received) { }

    public bool CheckCollision(BaseCollider collided) =>
        collided.ColliderBox.Overlaps(ColliderBox) && Plane == collided.Plane;

    public virtual bool CanCollideWithType(BaseCollider collider) => false;
    public virtual bool CanOverrideInvisibleDetection() => true;
    
    public string[] RunBaseCollisionDetection()
    {
        var colliders = Room.GetColliders();
        var collidedWith = new HashSet<string>();

        foreach (var collider in colliders)
        {
            if (CheckCollision(collider) && CanCollideWithType(collider) && collider.Active && (!collider.IsInvisible || CanOverrideInvisibleDetection()))
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }
        }

        return [.. collidedWith];
    }
}
