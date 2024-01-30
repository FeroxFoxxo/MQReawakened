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
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.TempData.Position.Z > 10 ? "Plane1" : "Plane0", player.Room, "player")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AIProjectileCollider)
        {
            var attack = (AIProjectileCollider)received;
            Console.WriteLine("holy fuck i just got shot");
        }
    }
}
