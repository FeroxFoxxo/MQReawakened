using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Entities.Colliders;

public class AIProjectileCollider(string id, string ownerId, Room room,
    Vector3Model position, RectModel size, string plane, float lifeTime, TimerThread timerThread, int damage, ItemEffectType effect,
    ItemCatalog itemCatalog, ServerRConfig serverRConfig) : BaseCollider
{
    public float LifeTime = lifeTime + room.Time;
    public string OwnderId => ownerId;
    public TimerThread TimerThread => timerThread;
    public int Damage => damage;
    public ItemEffectType Effect => effect;
    public ItemCatalog ItemCatalog => itemCatalog;
    public ServerRConfig ServerRConfig => serverRConfig;

    public override Room Room => room;
    public override string Id => id;
    public override Vector3Model Position => position;
    public override RectModel BoundingBox => size;
    public override string Plane => plane;
    public override ColliderType Type => ColliderType.AiAttack;

    public override string[] RunCollisionDetection(bool isAttack)
    {
        var colliders = Room.GetColliders();
        List<string> collidedWith = [];

        if (LifeTime <= Room.Time)
        {
            Room.Logger.LogTrace("Removing projectile collider {ColliderId} due to lifetime expiry.", Id);
            Room.RemoveCollider(Id);
            return ["0"];
        }

        foreach (var collider in colliders)
            if (CheckCollision(collider) && collider.Type != ColliderType.Attack &&
                collider.Type != ColliderType.AiAttack && collider.Type != ColliderType.Enemy &&
                collider.Type != ColliderType.Breakable && collider.Type != ColliderType.Hazard
                && collider.Active)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }

        return [.. collidedWith];
    }
}
