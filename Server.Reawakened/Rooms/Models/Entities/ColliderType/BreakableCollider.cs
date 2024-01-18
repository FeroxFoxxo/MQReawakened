using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Models.Planes;
using SmartFoxClientAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class BreakableCollider : BaseCollider
{
    public BreakableCollider(int id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : base(id, position, sizeX, sizeY, plane, room, "breakable") { }

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider)
        {
            var attack = (AttackCollider)received;
            Room.Entities.TryGetValue(Id, out var entity);
            foreach (var component in entity)
            {
                if (component is BreakableEventControllerComp breakable)
                    breakable.Damage(attack.Damage, attack.Owner);
            }
        }
    }
}
