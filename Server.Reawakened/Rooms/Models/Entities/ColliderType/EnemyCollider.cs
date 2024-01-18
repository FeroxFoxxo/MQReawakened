using Server.Reawakened.Rooms.Models.Planes;
using SmartFoxClientAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class EnemyCollider : BaseCollider
{
    public EnemyCollider(int id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : base(id, position, sizeX, sizeY, plane, room, "enemy") { }

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider)
        {
            var attack = (AttackCollider)received;
            Room.Enemies.TryGetValue(Id, out var enemy);
            enemy.Damage(attack.Damage, attack.Owner);
        }
    }
}
