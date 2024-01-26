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
public class EnemyBird(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{

    private float _behaviorEndTime;
    private float _initialDirection;

    public override void Initialize()
    {
        base.Initialize();

        BehaviorList = EnemyController.EnemyInfoXml.GetBehaviorsByName("PF_Critter_Bird");

        AiData.Intern_Dir = Generic.Patrol_ForceDirectionX;

        // Address magic numbers when we get to adding enemy effect mods
        Room.SendSyncEvent(AIInit(1, 1, 1));
        Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.Intern_Dir, false));

        // Set these calls to the xml later. Instead of using hardcoded "Patrol", "Aggro", etc.
        // the XML can just specify which behaviors to use when attacked, when moving, etc.
        AiBehavior = ChangeBehavior("Patrol");
    }

    public override void Damage(int damage, Player origin)
    {
        base.Damage(damage, origin);

        Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Shooting"), string.Empty, Position.x, Position.y,
             AiData.Intern_Dir, false));

        // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
        AiData.Sync_TargetPosX = origin.TempData.Position.X;
        AiData.Sync_TargetPosY = origin.TempData.Position.Y;

        if (AiBehavior is AIBehavior_Patrol)
        {
            AiBehavior.Stop(ref AiData);
            _initialDirection = AiData.Intern_Dir;
        }

        AiBehavior = ChangeBehavior("Shooting");
    }

    public override void HandlePatrol()
    {
        base.HandlePatrol();
    }
    //public override void HandleShooting()
    //{
    //    base.HandleShooting();
    //
    //    Console.WriteLine("hai!");
    //}
}
