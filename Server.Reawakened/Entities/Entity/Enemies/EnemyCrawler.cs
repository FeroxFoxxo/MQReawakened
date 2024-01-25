using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyCrawler(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{

    private float _behaviorEndTime;

    public override void Initialize()
    {
        base.Initialize();

        BehaviorList = EnemyController.EnemyInfoXml.GetBehaviorsByName("PF_Spite_Crawler");

        // Address magic numbers when we get to adding enemy effect mods
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.SyncInit_Dir, false));

        // Set these calls to the xml later. Instead of using hardcoded "Patrol", "Aggro", etc.
        // the XML can just specify which behaviors to use when attacked, when moving, etc.
        AiBehavior = ChangeBehavior("Patrol");
    }

    public override void Damage(int damage, Player origin)
    {
        base.Damage(damage, origin);

        Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Aggro"), string.Empty, origin.TempData.Position.X, origin.TempData.Position.Y,
            origin.TempData.Position.X > Position.x ? 1 : 0, false));
        AiData.SyncInit_Dir = origin.TempData.Position.X > Position.x ? 1 : 0;

        AiBehavior = ChangeBehavior("Aggro");
    }

    public override void HandlePatrol()
    {
        if (Entity.Id == "17745")
            Console.WriteLine("Patrolling!");
        base.HandlePatrol();
    }
    public override void HandleAggro()
    {
        base.HandleAggro();

        if (!AiBehavior.Update(ref AiData, Room.Time))
        {
            Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("LookAround"), string.Empty, Position.x, Position.y,
            AiData.SyncInit_Dir, false));

            AiBehavior = ChangeBehavior("LookAround");
            _behaviorEndTime = Room.Time + Convert.ToSingle(BehaviorList.GetBehaviorStat("LookAround", "lookTime"));
        }
    }
    public override void HandleLookAround()
    {
        base.HandleLookAround();

        if (Room.Time >= _behaviorEndTime)
        {
            Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.SyncInit_Dir, false));

            AiBehavior = ChangeBehavior("Patrol");
        }
    }
}
