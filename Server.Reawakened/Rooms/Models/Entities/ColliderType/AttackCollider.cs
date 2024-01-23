using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;
using SmartFoxClientAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class AttackCollider : BaseCollider
{
    public float LifeTime;
    public Player Owner;
    public int Damage;
    public Elemental Type;
    public AttackCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Player player, int damage, Elemental type, float lifeTime) : base(id, position, sizeX, sizeY, plane, player.Room, "attack")
    {
        Owner = player;
        Damage = damage;
        Type = type;
        LifeTime = lifeTime;
    }

    public override string[] IsColliding(bool isAttack)
    {
        var roomList = Room.Colliders.Values.ToList();
        List<string> collidedWith = new List<string>();

        if (isAttack)
        {
            foreach (var collider in roomList)
            {
                if (CheckCollision(collider) && !collider.ColliderType.Equals("attack"))
                {
                    collidedWith.Add(collider.Id);
                    collider.SendCollisionEvent(this);
                }
            }
        }
        return collidedWith.ToArray();
    }
}
