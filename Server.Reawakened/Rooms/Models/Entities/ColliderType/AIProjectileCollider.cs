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
public class AIProjectileCollider(string ownerId, Room room, string id, Vector3Model position, float sizeX, float sizeY, string plane, float lifeTime) : BaseCollider(id, position, sizeX, sizeY, plane, room, "aiattack")
{
    public float LifeTime = lifeTime + room.Time;
    public string Owner = ownerId;

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
        {
            if (CheckCollision(collider) && collider.ColliderType != "attack" && collider.ColliderType != "aiattack" && collider.ColliderType != "enemy")
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }
        }

        return [.. collidedWith];
    }
}
