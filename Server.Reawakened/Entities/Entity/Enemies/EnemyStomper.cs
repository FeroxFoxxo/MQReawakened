﻿using A2m.Server;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyStomper(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{

    private float _behaviorEndTime;
    private float _initialDirection;

    public override void Initialize()
    {
        base.Initialize();

        BehaviorList = EnemyController.EnemyInfoXml.GetBehaviorsByName(Entity.PrefabName);

        MinBehaviorTime = Convert.ToSingle(BehaviorList.GetGlobalProperty("MinBehaviorTime"));
        EnemyGlobalProps.Global_DetectionLimitedByPatrolLine = Convert.ToBoolean(BehaviorList.GetGlobalProperty("DetectionLimitedByPatrolLine"));
        EnemyGlobalProps.Global_FrontDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeX"));
        EnemyGlobalProps.Global_FrontDetectionRangeUpY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeUpY"));
        EnemyGlobalProps.Global_FrontDetectionRangeDownY = Convert.ToSingle(BehaviorList.GetGlobalProperty("FrontDetectionRangeDownY"));
        EnemyGlobalProps.Global_BackDetectionRangeX = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeX"));
        EnemyGlobalProps.Global_BackDetectionRangeUpY = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeUpY"));
        EnemyGlobalProps.Global_BackDetectionRangeDownY = Convert.ToSingle(BehaviorList.GetGlobalProperty("BackDetectionRangeDownY"));

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

        if (AiBehavior is not AIBehavior_Stomper)
        {

            Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Aggro"), string.Empty, origin.TempData.Position.X, origin.TempData.Position.Y,
                 AiData.Intern_Dir, false));

            // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
            AiData.Sync_TargetPosX = origin.TempData.Position.X;
            AiData.Sync_TargetPosY = origin.TempData.Position.Y;

            if (AiBehavior is AIBehavior_Patrol)
            {
                AiBehavior.Stop(ref AiData);
                _initialDirection = AiData.Intern_Dir;
            }

            AiBehavior = ChangeBehavior("Aggro");
            _behaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
        }
    }

    public override void HandlePatrol()
    {
        base.HandlePatrol();
        DetectPlayers("Aggro");
    }

    public override void HandleAggro()
    {
        base.HandleAggro();

        if (!AiBehavior.Update(ref AiData, Room.Time))
        {
            Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Stomper"), string.Empty, AiData.Sync_TargetPosX, AiData.Sync_TargetPosY,
            AiData.Intern_Dir, false));

            AiBehavior = ChangeBehavior("Stomper");
            _behaviorEndTime = ResetBehaviorTime(Convert.ToSingle(BehaviorList.GetBehaviorStat("Stomper", "attackTime")));
        }
    }

    public override void HandleStomper()
    {
        base.HandleStomper();
        //DetectPlayers("Aggro");
        if (Room.Time >= _behaviorEndTime)
        {
            Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf("Patrol"), string.Empty, Position.x, Position.y, AiData.Intern_Dir, false));

            AiBehavior = ChangeBehavior("Patrol");
        }
    }

    public override void DetectPlayers(string behaviorToRun)
    {
        foreach (var player in Room.Players)
        {
            if (PlayerInRange(player.Value.TempData.Position, EnemyGlobalProps.Global_DetectionLimitedByPatrolLine))
            {
                Room.SendSyncEvent(SyncBuilder.AIDo(Entity, Position, 1.0f, BehaviorList.IndexOf(behaviorToRun), string.Empty, player.Value.TempData.Position.X,
                    Position.y, Generic.Patrol_ForceDirectionX, false));

                // For some reason, the SyncEvent doesn't initialize these properly, so I just do them here
                AiData.Sync_TargetPosX = player.Value.TempData.Position.X;
                AiData.Sync_TargetPosY = Position.y;

                AiBehavior = ChangeBehavior(behaviorToRun);

                _behaviorEndTime = ResetBehaviorTime(MinBehaviorTime);
            }
        }
    }
}
