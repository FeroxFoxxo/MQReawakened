using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class AIProjectileCollider(string projectileId, string ownerId, Room room, string id, Vector3Model position,
    float sizeX, float sizeY, string plane, float lifeTime, TimerThread timerThread, int damage, ItemEffectType effect,
    ItemCatalog itemCatalog) : BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderType.AiAttack)
{
    public float LifeTime = lifeTime + room.Time;
    public string PrjId = projectileId;
    public string OwnderId = ownerId;
    public TimerThread TimerThread = timerThread;
    public int Damage = damage;
    public ItemEffectType Effect = effect;
    public ItemCatalog ItemCatalog = itemCatalog;

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
                collider.Type != ColliderType.Breakable && collider.Type != ColliderType.Hazard)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }

        return [.. collidedWith];
    }
}
