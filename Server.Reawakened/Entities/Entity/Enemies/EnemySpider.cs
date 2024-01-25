using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemySpider(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{
    public override void Initialize()
    {
        base.Initialize();

        BehaviorList = EnemyController.EnemyInfoXml.GetBehaviorsByName("PF_Critter_Spider");

        AiBehavior = new AIBehavior_Patrol(
            Generic.Patrol_DistanceX,
            Generic.Patrol_DistanceY,
            Convert.ToSingle(BehaviorList.GetBehaviorStat("Patrol", "speed")),
            Convert.ToSingle(BehaviorList.GetBehaviorStat("Patrol", "endPathWaitTime")),
            Generic.Patrol_ForceDirectionX,
            Generic.Patrol_InitialProgressRatio
        );

        AiBehavior.Start(ref AiData, Room.Time, []);

        // Address magic numbers when we get to adding enemy effect mods
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(SyncBuilder.AIDo(Entity, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.SyncInit_Dir, false));
    }

    public override void Damage(int damage, Player origin)
    {
        base.Damage(damage, origin);
        if (AiBehavior is not AIBehavior_Aggro)
        {
            AiBehavior = new AIBehavior_Aggro(
                Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "speed")),
                Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "moveBeyondTargetDistance")),
                Convert.ToBoolean(BehaviorList.GetBehaviorStat("Aggro", "stayOnPatrolPath")),
                Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "attackBeyondPatrolLine")),
                Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "detectionHeightUpY")),
                Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "detectionHeightDownY"))
            );
            AiBehavior.Start(ref AiData, Room.Time, []);

            Room.SendSyncEvent(SyncBuilder.AIDo(Entity, 1.0f, BehaviorList.IndexOf("Aggro"), string.Empty, Position.x, Position.y, AiData.SyncInit_Dir, false));
        }

    }

    public override void HandlePatrol()
    {
        base.HandlePatrol();
    }
}
