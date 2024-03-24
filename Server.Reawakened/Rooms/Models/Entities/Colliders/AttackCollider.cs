using A2m.Server;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class AttackCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Player player, int damage, Elemental type, float lifeTime, float offset) : BaseCollider(id, position, sizeX, sizeY, plane, player.Room, ColliderClass.Attack)
{
    public float LifeTime = player.Room.Time + lifeTime;
    public Player Owner = player;
    public int Damage = damage;
    public Elemental DamageType = type;

    private float _offset = player.Room.Time + offset;

    public override string[] IsColliding(bool isAttack)
    {
        var roomList = Room.Colliders.Values.ToList();
        List<string> collidedWith = [];

        if (LifeTime <= Room.Time)
        {
            Room.Colliders.Remove(Id);
            return ["0"];
        }

        if (isAttack)
        {
            foreach (var collider in roomList)
            {
                if (CheckCollision(collider) &&
                    collider.Type != ColliderClass.Attack && collider.Type != ColliderClass.Player &&
                    collider.Type != ColliderClass.Hazard && collider.Type != ColliderClass.AiAttack
                    && Room.Time >= _offset)
                {
                    collidedWith.Add(collider.Id);
                    collider.SendCollisionEvent(this);
                }
            }
        }

        return [.. collidedWith];
    }
}
