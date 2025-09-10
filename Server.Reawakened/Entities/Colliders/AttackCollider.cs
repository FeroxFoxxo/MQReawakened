using A2m.Server;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class AttackCollider(string id, Vector3Model position,
    RectModel box, string plane, Player player,
    int damage, Elemental type, float lifeTime, float offset, bool canSeeInvis) : BaseCollider
{
    public Player Owner => player;
    public int Damage => damage;
    public Elemental DamageType => type;
    public bool CanSeeInvisible => canSeeInvis;

    public override Room Room => player.Room;
    public override string Id => id;
    public override Vector3Model Position => position;
    public override RectModel BoundingBox => box;
    public override string Plane => plane;
    public override ColliderType Type => ColliderType.Attack;

    public readonly float OffsetTime = player.Room.Time + offset;
    public readonly float LifeTime = player.Room.Time + lifeTime;

    public override string[] RunCollisionDetection(bool isAttack)
    {
        var colliders = Room.GetColliders();

        var collidedWith = new HashSet<string>();

        var time = Room.Time;

        if (LifeTime <= time)
        {
            Room.RemoveCollider(Id);
            return [];
        }

        if (time < OffsetTime)
            return [];

        if (isAttack)
            foreach (var collider in colliders)
            {
                var isCollidable = collider.Type is not ColliderType.Attack and not
                    ColliderType.Player and not ColliderType.Hazard and not ColliderType.AiAttack
                    && collider.Active && (!collider.IsInvisible || CanSeeInvisible);

                if (!isCollidable)
                    continue;

                var collided = CheckCollision(collider);

                if (!collided)
                    continue;

                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }

        return [.. collidedWith];
    }
}
