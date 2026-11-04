using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class AIProjectileCollider(string id, string ownerId, Room room,
    Vector3Model position, RectModel size, string plane, float lifeTime, ItemEffectType effect) : BaseCollider
{
    public float LifeTime = lifeTime + room.Time;
    public string OwnderId => ownerId;
    public ItemEffectType Effect => effect;

    public override Room Room => room;
    public override string Id => id;
    public override Vector3Model Position => position;
    public override RectModel BoundingBox => size;
    public override string Plane => plane;
    public override ColliderType Type => ColliderType.AiAttack;

    public override bool CanCollideWithType(BaseCollider collider) =>
        collider.Type switch
        {
            ColliderType.TerrainCube => true,
            ColliderType.Player => true,
            ColliderType.TriggerReceiver => true,
            ColliderType.Breakable => true,
            ColliderType.Mesh => true,
            ColliderType.Default => true,
            _ => false
        };

    public override string[] RunCollisionDetection()
    {
        if (LifeTime <= Room.Time)
        {
            Room.Logger.LogTrace("Removing projectile collider {ColliderId} due to lifetime expiry.", Id);
            Room.RemoveCollider(Id);
            return [];
        }

        return RunBaseCollisionDetection();
    }
}
