using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class AIProjectileCollider(string projectileId, string ownerId, Room room, string id,
    Vector3 position, Rect size, string plane, float lifeTime, TimerThread timerThread, int damage, ItemEffectType effect,
    ItemCatalog itemCatalog, ItemRConfig itemConfig, ServerRConfig serverRConfig) : BaseCollider(id, position, size, plane, room, ColliderType.AiAttack)
{
    public float LifeTime = lifeTime + room.Time;
    public string PrjId = projectileId;
    public string OwnderId = ownerId;
    public TimerThread TimerThread = timerThread;
    public int Damage = damage;
    public ItemEffectType Effect = effect;
    public ItemCatalog ItemCatalog = itemCatalog;
    public ItemRConfig ItemConfig = itemConfig;
    public ServerRConfig ServerRConfig = serverRConfig;

    public override string[] IsColliding(bool isAttack)
    {
        var colliders = Room.GetColliders();
        List<string> collidedWith = [];

        if (LifeTime <= Room.Time)
        {
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
