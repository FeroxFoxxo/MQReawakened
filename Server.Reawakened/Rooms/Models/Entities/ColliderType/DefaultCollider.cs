using Server.Reawakened.Rooms.Models.Planes;
using SmartFoxClientAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class DefaultCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, position, sizeX, sizeY, plane, room, "default")
{
}
