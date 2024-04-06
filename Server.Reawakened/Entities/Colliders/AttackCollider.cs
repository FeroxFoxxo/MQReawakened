using A2m.Server;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class AttackCollider(string id, Vector3Model position,
    Vector2 size, string plane, Player player,
    int damage, Elemental type, float lifeTime, float offset) :
    BaseCollider(id, position, size, plane, player.Room, ColliderType.Attack)
{
    public float LifeTime = player.Room.Time + lifeTime;
    public Player Owner = player;
    public int Damage = damage;
    public Elemental DamageType = type;

    public readonly float OffsetTime = player.Room.Time + offset;

    public override string[] IsColliding(bool isAttack)
    {
        var colliders = Room.GetColliders();

        List<string> collidedWith = [];

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
                var collided = CheckCollision(collider);
                var isCollidableType = collider.Type is not ColliderType.Attack and not
                    ColliderType.Player and not ColliderType.Hazard and not ColliderType.AiAttack;

                if (collided && isCollidableType)
                {
                    collidedWith.Add(collider.Id);
                    collider.SendCollisionEvent(this);
                }
            }

        return [.. collidedWith];
    }
}
