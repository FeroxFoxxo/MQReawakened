using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Entities.Colliders;

public class AttackCollider(string id, Vector3Model position,
    RectModel box, string plane, Player player,
    ItemDescription item, List<ItemEffect> itemEffects, string prefabFrom, float lifeTime, float offset, bool canSeeInvis, ItemCatalog itemCatalog, float dmgScale = 1) : BaseCollider
{
    public Player Owner => player;
    public ItemDescription Item => item;
    public List<ItemEffect> ItemEffects => itemEffects;
    public string PrefabFrom => prefabFrom;
    public bool CanSeeInvisible => canSeeInvis;
    public ItemCatalog ItemCatalog => itemCatalog;

    public override Room Room => player.Room;
    public override string Id => id;
    public override Vector3Model Position => position;
    public override RectModel BoundingBox => box;
    public override string Plane => plane;
    public override ColliderType Type => ColliderType.Attack;

    public readonly float OffsetTime = player.Room.Time + offset;
    public readonly float LifeTime = player.Room.Time + lifeTime;

    public float DamageScale => dmgScale;

    public override bool CanOverrideInvisibleDetection() => CanSeeInvisible;

    public override bool CanCollideWithType(BaseCollider collider) =>
        collider.Type switch
        {
            ColliderType.Enemy => true,
            ColliderType.TriggerTarget => true,
            ColliderType.Breakable => true,
            ColliderType.TerrainCube => true,
            ColliderType.Default => true,
            _ => false
        };

    public override string[] RunCollisionDetection()
    {
        if (LifeTime <= Room.Time)
        {
            Room.Logger.LogTrace("Removing attack collider {ColliderId} due to lifetime expiry.", Id);
            Room.RemoveCollider(Id);
            return [];
        }

        return Room.Time < OffsetTime ? [] : RunBaseCollisionDetection();
    }
}
