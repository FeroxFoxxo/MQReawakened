using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyGeneric(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{
}
