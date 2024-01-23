using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity.Enemies;
public class EnemyPincer(Room room, string entityId, BaseComponent baseEntity) : Enemy(room, entityId, baseEntity)
{
    public float AggroSpeed;
    public float TargetPosX;

    public override void Initialize()
    {
    }

    public override void Update()
    {
        if (!(Behavior is AIBehavior_Aggro))
        {
            foreach (var player in Room.Players.ToList())
            {
                var pos = player.Value.TempData.Position;

                //Add method to detect if the player is dead. If player is dead, enemy does not target
                if (PlayerInRange(pos))
                {
                    BehaviorTime = Room.Time + 3;
                    TargetPosX = pos.X;
                    AiData.Sync_TargetPosX = pos.X;
                    AiData.Sync_TargetPosY = Position.y;
                    AiData.Sync_PosX = Position.x;
                    Room.SendSyncEvent(AIDo(1.0f, 2, "", pos.X, Position.y, AiData.SyncInit_Dir, 1));
                    Behavior = new AIBehavior_Aggro(AggroSpeed, 0, false, 0, 4, 1);
                    Behavior.Start(ref AiData, Room.Time, [""]);
                }
            }
        }
        else if (Behavior is AIBehavior_Aggro)
        {
            if (AiData.Intern_BehaviorRequestTime == 1)
            {
                foreach (var player in Room.Players.ToList())
                {
                    var pos = player.Value.TempData.Position;

                    //Add method to detect if the player is dead. If player is dead, enemy does not target
                    if (!PlayerInRange(pos))
                    {
                        Room.SendSyncEvent(AIDo(1.0f, 3, "", Position.x, Position.y, AiData.SyncInit_Dir, 0));
                        AiData.Intern_BehaviorRequestTime = Room.Time + 2;
                        Behavior = new AIBehavior_LookAround(2, Global.LookAround_InitialProgressRatio, false);
                    }
                }
            }
        }
        if (Behavior is AIBehavior_LookAround)
        {
            if (Room.Time >= AiData.Intern_BehaviorRequestTime)
            {
                Room.SendSyncEvent(AIDo(1.0f, 1, "", Position.x, Position.y, AiData.SyncInit_Dir, 0));
                Behavior = new AIBehavior_Patrol(
            Generic.Patrol_DistanceX,
            Generic.Patrol_DistanceY,
            PatrolSpeed,
            EndPathWaitTime,
            Generic.Patrol_ForceDirectionX,
            Generic.Patrol_InitialProgressRatio
        );
            }
        }
        base.Update();
    }

    public override string WriteBehaviorList()
    {
        string output = "Idle||";
        List<string> behaviorList = [];

        PatrolSpeed = 2.4f;
        AggroSpeed = 3.2f;
        AiData.Sync_SpeedFactor = AggroSpeed;
        AiData.Intern_MinPointX = SpawnPosition.x;
        AiData.Intern_MaxPointX = SpawnPosition.x + Generic.Patrol_DistanceX;
        AiData.Intern_MinPointY = Position.y;
        AiData.Intern_MaxPointY = Position.y;
        EndPathWaitTime = 0;
        //temp values for now. make this based on DetectionRange soon
        DetectionRange = new Rect(Position.x - 5, Position.y, 10, 4);
        behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        behaviorList.Add("Aggro|" + AggroSpeed + ";" + 0 + ";" + 0 + ";" + 0 + ";" + 0 + ";" + 2 + ";" + 1 + "|");
        behaviorList.Add("LookAround|" + 2 + ";" + AiData.SyncInit_Dir + ";" + Global.LookAround_ForceDirection + ";" + Global.LookAround_InitialProgressRatio + ";" + 0 + "|");
        foreach (var bah in behaviorList)
        {
            if (behaviorList.Count > 0)
                output = output + "`" + bah;
        }

        Behavior = new AIBehavior_Patrol(
            Generic.Patrol_DistanceX,
            Generic.Patrol_DistanceY,
            PatrolSpeed,
            EndPathWaitTime,
            Generic.Patrol_ForceDirectionX,
            Generic.Patrol_InitialProgressRatio
        );

        return output;
    }
}
