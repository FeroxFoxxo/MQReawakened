using A2m.Server;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class AIProjectileCollider(string projectileId, string ownerId, Room room, string id, Vector3Model position, float sizeX, float sizeY, string plane, float lifeTime, TimerThread timerThread, int damage, ItemEffectType effect, ItemCatalog itemCatalog) : BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderClass.AiAttack)
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
        var roomList = Room.Colliders.Values.ToList();
        List<string> collidedWith = [];

        if (LifeTime <= Room.Time)
        {
            Room.Colliders.Remove(Id);
            return ["0"];
        }

        foreach (var collider in roomList)
            if (CheckCollision(collider) && collider.Type != ColliderClass.Attack &&
                collider.Type != ColliderClass.AiAttack && collider.Type != ColliderClass.Enemy &&
                collider.Type != ColliderClass.Breakable && collider.Type != ColliderClass.Hazard)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }

        return [.. collidedWith];
    }
}
