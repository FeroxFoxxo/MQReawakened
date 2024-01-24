using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyOrchid(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{
    public override void Initialize()
    {
        base.Initialize();

        BehaviorList = EnemyController.EnemyInfoXml.GetBehaviorsByName("PF_Spite_Orchid");

        AiBehavior = new AIBehavior_Patrol(
            Generic.Patrol_DistanceX,
            Generic.Patrol_DistanceY,
            (float)BehaviorList.GetBehaviorStat("Patrol", "speed"),
            (float)BehaviorList.GetBehaviorStat("Patrol", "endPathWaitTime"),
            Generic.Patrol_ForceDirectionX,
            Generic.Patrol_InitialProgressRatio
        );

        // Address magic numbers when we get to adding enemy effect mods
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(SyncBuilder.AIDo(Entity, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.SyncInit_Dir, false));
    }

    public override void Update()
    {
        base.Update();
    }

    public override void HandlePatrol()
    {
        base.HandlePatrol();
        foreach (var player in Room.Players.ToList())
        {
            var pos = player.Value.TempData.Position;
            if (PlayerInRange(pos))
            {
                Room.SendSyncEvent(SyncBuilder.AIDo(Entity, 1.0f, BehaviorList.IndexOf("Aggro"), string.Empty, Position.x, Position.y, Generic.Patrol_ForceDirectionX, false));
            }
        }
    }
}
