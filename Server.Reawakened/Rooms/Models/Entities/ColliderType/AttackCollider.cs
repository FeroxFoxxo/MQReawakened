using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;

public class AttackCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane,
    Player player, int damage, Elemental type, float lifeTime) : BaseCollider(id, position, sizeX, sizeY, plane, player.Room, "attack")
{
    public float LifeTime = lifeTime;
    public Player Owner = player;
    public int Damage = damage;
    public Elemental Type = type;

    public override string[] IsColliding(bool isAttack)
    {
        var roomList = Room.Colliders.Values.ToList();
        var collidedWith = new List<string>();

        if (isAttack)
            foreach (var collider in roomList)
                if (CheckCollision(collider) && !collider.ColliderType.Equals("attack"))
                {
                    collidedWith.Add(collider.Id);
                    collider.SendCollisionEvent(this);
                }

        return [.. collidedWith];
    }
}
