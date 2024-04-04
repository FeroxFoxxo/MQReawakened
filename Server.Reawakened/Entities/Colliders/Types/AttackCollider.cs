using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class AttackCollider(string id, Vector3Model position,
    float sizeX, float sizeY, string plane, Player player,
    int damage, Elemental type, float lifeTime, float offset) :
    BaseCollider(id, position, sizeX, sizeY, plane, player.Room, ColliderType.Attack)
{
    public float LifeTime = player.Room.Time + lifeTime;
    public Player Owner = player;
    public int Damage = damage;
    public Elemental DamageType = type;

    private readonly float _offset = player.Room.Time + offset;

    public override string[] IsColliding(bool isAttack)
    {
        var colliders = Room.GetColliders();

        List<string> collidedWith = [];

        if (LifeTime <= Room.Time)
        {
            Room.RemoveCollider(Id);
            return ["0"];
        }

        if (isAttack)
            foreach (var collider in colliders)
                if (CheckCollision(collider) &&
                    collider.Type != ColliderType.Attack && collider.Type != ColliderType.Player &&
                    collider.Type != ColliderType.Hazard && collider.Type != ColliderType.AiAttack
                    && Room.Time >= _offset)
                {
                    collidedWith.Add(collider.Id);
                    collider.SendCollisionEvent(this);
                }

        return [.. collidedWith];
    }
}
