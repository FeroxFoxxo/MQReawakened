using Server.Base.Core.Extensions;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Stats;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace Server.Reawakened.Entities.Entity;
public abstract class Enemy : IDestructible
{
    public bool Init;
    public Room Room;
    public int Id;
    public Vector3 SpawnPosition;
    public Vector3 Position;
    public Rect DetectionRange;
    public BaseCollider Hitbox;
    public string ParentPlane;
    public float NegativeHeight;
    public float BehaviorTime;
    public int Health;

    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public InterObjStatusComp Status;
    public BaseComponent Entity;

    public AIProcessData AiData;
    public GlobalProperties EnemyGlobalProps;
    public AIBaseBehavior Behavior;
    public float PatrolSpeed;
    public float EndPathWaitTime;
    public int[] HitByAttackList;

    public WorldGraph WorldGraph { get; set; }

    public Enemy(Room room, int entityId, BaseComponent baseEntity)
    {
        Entity = baseEntity;
        Room = room;
        Id = entityId;
        //temp
        Health = 80;
        ParentPlane = Entity.ParentPlane;
        Position = new Vector3(Entity.Position.X, Entity.Position.Y, Entity.Position.Z);
        if (ParentPlane == "Plane1")
            Position.z = 20;
        SpawnPosition = Position;

        var entityList = room.Entities.Values.SelectMany(s => s);
        foreach (var entity in entityList)
        {
            if (entity.Id == Id && entity is AIStatsGlobalComp global)
                Global = global;
            else if (entity.Id == Id && entity is AIStatsGenericComp generic)
                Generic = generic;
            else if (entity.Id == Id && entity is InterObjStatusComp status)
                Status = status;
        }

        EnemyGlobalProps = new GlobalProperties(
            Global.Global_DetectionLimitedByPatrolLine,
            Global.Global_BackDetectionRangeX,
            Global.Global_viewOffsetY,
            Global.Global_BackDetectionRangeUpY,
            Global.Global_BackDetectionRangeDownY,
            Global.Global_ShootOffsetX,
            Global.Global_ShootOffsetY,
            Global.Global_FrontDetectionRangeX,
            Global.Global_FrontDetectionRangeUpY,
            Global.Global_FrontDetectionRangeDownY,
            Global.Global_Script,
            Global.Global_ShootingProjectilePrefabName,
            Global.Global_DisableCollision,
            Global.Global_DetectionSourceOnPatrolLine,
            Global.Aggro_AttackBeyondPatrolLine
        );

        AiData = new AIProcessData();
        AiData.SetStats(EnemyGlobalProps);
        AiData.SyncInit_PosX = Position.x;
        AiData.SyncInit_PosY = Position.y;
        AiData.Sync_PosX = Position.x;
        AiData.Sync_PosY = Position.y;
        AiData.Intern_SpawnPosX = Position.x;
        AiData.Intern_SpawnPosY = Position.y;
        AiData.Intern_SpawnPosZ = Position.z;
        AiData.SyncInit_Dir = Generic.Patrol_ForceDirectionX;
        AiData.SyncInit_ProgressRatio = Generic.Patrol_InitialProgressRatio;

        NegativeHeight = 0;
        if (Entity.Scale.Y < 0)
            NegativeHeight = Entity.Rectangle.Height;
        Hitbox = new EnemyCollider(Id, new Vector3Model { X = Position.x, Y = Position.y - NegativeHeight, Z = Position.z }, Entity.Rectangle.Width, Entity.Rectangle.Height, Entity.ParentPlane, Room);
        Room.Colliders.Add(Id, Hitbox);
    }
    public virtual void Initialize()
    {
    }

    public virtual void Update()
    {
        if (!Init)
        {
            // Address magic numbers when we get around to adding enemy effect mods
            Room.SendSyncEvent(AIInit(1, 1, 1));

            // Address first magic number when we get to adding enemy effect mods
            Room.SendSyncEvent(AIDo(1.0f, 1, "", Position.x, Position.y, AiData.SyncInit_Dir, 0));
            Init = true;
        }

        Behavior.Update(AiData, Room.Time);

        Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY, Position.z);

        //Hitbox stuff
        Hitbox.Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY - NegativeHeight, Position.z);

    }

    public virtual string WriteBehaviorList()
    {
        string output = "Idle||";
        List<string> behaviorList = [];

        if (Entity.PrefabName.Contains("PF_Critter_Bird"))
        {
            PatrolSpeed = 3.2f;
            EndPathWaitTime = 0;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Critter_Spider"))
        {
            PatrolSpeed = 5.0f;
            EndPathWaitTime = 2;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Critter_Fish"))
        {
            PatrolSpeed = 3.2f;
            EndPathWaitTime = 2;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Spite_Bathog"))
        {
            PatrolSpeed = 1.8f;
            EndPathWaitTime = 3;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Spite_Bomber"))
        {
            PatrolSpeed = 1.8f;
            EndPathWaitTime = 2;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Spite_Crawler"))
        {
            PatrolSpeed = 1.8f;
            EndPathWaitTime = 3;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
            behaviorList.Add("Aggro|" + 3.2 + ";" + Global.Aggro_MoveBeyondTargetDistance + ";" + 0 + ";" + Global.Aggro_AttackBeyondPatrolLine + ";" + 0 + ";" + Global.Global_FrontDetectionRangeUpY + ";" + Global.Global_FrontDetectionRangeDownY + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Spite_Dragon"))
        {
            PatrolSpeed = 1.8f;
            EndPathWaitTime = 2;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }
        else if (Entity.PrefabName.Contains("PF_Spite_Stomper"))
        {
            PatrolSpeed = 1.5f;
            EndPathWaitTime = 4;
            behaviorList.Add("Patrol|" + PatrolSpeed + ";" + 0 + ";" + EndPathWaitTime + ";" + Generic.Patrol_DistanceX + ";" + Generic.Patrol_DistanceY + ";" + Generic.Patrol_ForceDirectionX + ";" + Generic.Patrol_InitialProgressRatio + "|");
        }

        foreach (var bah in behaviorList)
        {
            if (behaviorList.Count > 0)
                output = output + "`" + bah;
        }

        Behavior = new AIBehavior_Patrol(new Vector3(SpawnPosition.x, SpawnPosition.y, SpawnPosition.z),
        new Vector3(SpawnPosition.x + Generic.Patrol_DistanceX, SpawnPosition.y + Generic.Patrol_DistanceY, SpawnPosition.z),
        PatrolSpeed,
        EndPathWaitTime);

        return output;
    }

    public virtual void Damage(int damage, Player origin)
    {
        Health -= damage;
        var damageEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health, damage, 0, 0, origin.CharacterName, false, true);
        Room.SendSyncEvent(damageEvent);
        if (Health <= 0)
        {
            var kill = new SyncEvent(Id.ToString(), SyncEvent.EventType.AIDie, Room.Time);
            kill.EventDataList.Add("");
            kill.EventDataList.Add(10);
            kill.EventDataList.Add(1);
            kill.EventDataList.Add(origin.GameObjectId.ToString());
            kill.EventDataList.Add(0);
            Destroy(Room, Id);
        }
    }

    public virtual bool PlayerInRange(Vector3Model pos)
    {
        if (Position.x - DetectionRange.width / 2 < pos.X && pos.X < Position.x + DetectionRange.width / 2 &&
            Position.y < pos.Y && pos.Y < Position.y + DetectionRange.height && Position.z == pos.Z)
            return true;
        return false;
    }
    public virtual AIDo_SyncEvent AIDo(float speedFactor, int behaviorId, string args, float targetPosX, float targetPosY, int direction, int awareBool)
    {
        var aiDo = new AIDo_SyncEvent(new SyncEvent(Id.ToString(), SyncEvent.EventType.AIDo, Room.Time));
        aiDo.EventDataList.Clear();
        aiDo.EventDataList.Add(Position.x);
        aiDo.EventDataList.Add(Position.y);
        aiDo.EventDataList.Add(speedFactor);
        aiDo.EventDataList.Add(behaviorId);
        aiDo.EventDataList.Add(args);
        aiDo.EventDataList.Add(targetPosX);
        aiDo.EventDataList.Add(targetPosY);
        aiDo.EventDataList.Add(direction);
        // 0 for false, 1 for true.
        aiDo.EventDataList.Add(awareBool);
        return aiDo;
    }

    public virtual AIInit_SyncEvent AIInit(float healthMod, float sclMod, float resMod)
    {
        var aiInit = new AIInit_SyncEvent(Id.ToString(), Room.Time, Position.x, Position.y, Position.z, Position.x, Position.y, Generic.Patrol_InitialProgressRatio,
            Status.MaxHealth, Status.MaxHealth, healthMod, sclMod, resMod, Status.Stars, Status.GenericLevel, EnemyGlobalProps.ToString(), WriteBehaviorList());
        aiInit.EventDataList[2] = Position.x;
        aiInit.EventDataList[3] = Position.y;
        aiInit.EventDataList[4] = Position.z;
        return aiInit;
    }

    public virtual AILaunchItem_SyncEvent AILaunchItem(float posX, float posY, float posZ, float speedX, float speedY, float lifeTime, int prjId, int isGrenade)
    {
        var launch = new AILaunchItem_SyncEvent(new SyncEvent(Id.ToString(), SyncEvent.EventType.AILaunchItem, Room.Time));
        launch.EventDataList.Clear();
        launch.EventDataList.Add(Position.x);
        launch.EventDataList.Add(Position.y);
        launch.EventDataList.Add(Position.z);
        launch.EventDataList.Add(speedX);
        launch.EventDataList.Add(speedY);
        launch.EventDataList.Add(lifeTime);
        launch.EventDataList.Add(prjId);
        launch.EventDataList.Add(isGrenade);
        return launch;
    }

    public void Destroy(Room room, int id)
    {
        room.Entities.Remove(id);
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
