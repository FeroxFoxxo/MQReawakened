using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity.Utils;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Enums;
using UnityEngine;

namespace Server.Reawakened.Entities.Entity;

public abstract class Enemy : IDestructible
{
    public bool Init;
    public Room Room;
    public string Id;

    public Vector3 Position;
    public Rect DetectionRange;
    public EnemyCollider Hitbox;
    public string ParentPlane;
    public int Health;
    public bool IsFromSpawner;
    public float MinBehaviorTime;

    private readonly float _negativeHeight;

    public BaseComponent Entity;
    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public InterObjStatusComp Status;
    public EnemyControllerComp EnemyController;

    public GlobalProperties EnemyGlobalProps;
    public AIProcessData AiData;
    public AIBaseBehavior AiBehavior;
    public BehaviorModel BehaviorList;

    public AISyncEventHelper SyncBuilder = new();
    public ILogger<Enemy> Logger { get; set; }

    public Enemy(Room room, string entityId, BaseComponent baseEntity)
    {
        //Basic Stats
        Room = room;
        Id = entityId;
        Health = 50;
        IsFromSpawner = false;
        MinBehaviorTime = 0;

        //Component Info
        Entity = baseEntity;
        EnemyController = (EnemyControllerComp)baseEntity;

        var global = room.GetEntityFromId<AIStatsGlobalComp>(Id);

        if (global != null)
            Global = global;

        var generic = room.GetEntityFromId<AIStatsGenericComp>(Id);

        if (generic != null)
            Generic = generic;

        var status = room.GetEntityFromId<InterObjStatusComp>(Id);

        if (status != null)
            Status = status;
        
        //Position Info
        ParentPlane = Entity.ParentPlane;
        Position = new Vector3(Entity.Position.X, Entity.Position.Y, Entity.Position.Z);

        if (ParentPlane == "Plane1")
            Position.z = 20;

        //Hitbox Info
        _negativeHeight = 0;

        if (Entity.Scale.Y < 0)
            _negativeHeight = Entity.Rectangle.Height;

        Hitbox = new EnemyCollider(Id, new Vector3Model { X = Position.x, Y = Position.y - _negativeHeight, Z = Position.z },
            Entity.Rectangle.Width, Entity.Rectangle.Height, Entity.ParentPlane, Room);
        Room.Colliders.Add(Id, Hitbox);

        //GlobalProperties assignment, used for AI_Behavior
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

        //AIProcessData assignment, used for AI_Behavior
        AiData = new AIProcessData();
        AiData.SetStats(EnemyGlobalProps);
        AiData.Intern_SpawnPosX = Position.x;
        AiData.Intern_SpawnPosY = Position.y;
        AiData.Intern_SpawnPosZ = Position.z;
        AiData.Sync_PosX = Position.x;
        AiData.Sync_PosY = Position.y;
        AiData.SyncInit_Dir = Generic.Patrol_ForceDirectionX;
        AiData.SyncInit_ProgressRatio = Generic.Patrol_InitialProgressRatio;
    }

    public virtual void Initialize() => Init = true;

    public virtual void Update()
    {
        if (!Init)
            Initialize();

        switch (AiBehavior)
        {
            //All commented lines are behaviors that have not been added yet

            //AIBehavior_Acting
            case AIBehaviorLookAround:
                HandleLookAround();
                break;
            case AIBehaviorPatrol:
                HandlePatrol();
                break;
            case AIBehaviorAggro:
                HandleAggro();
                break;
            case AIBehaviorComeBack:
                HandleComeBack();
                break;
            case AIBehaviorShooting:
                HandleShooting();
                break;
            //AIBehavior_Projectile
            case AIBehaviorBomber:
                HandleBomber();
                break;
            case AIBehaviorGrenadier:
                HandleGrenadier();
                break;
            case AIBehaviorStomper:
                HandleStomper();
                break;
            case AIBehaviorStinger:
                HandleStinger();
                break;
                //AIBehavior_Spike
        }

        //if (Id.Equals("28489"))
        //    Console.WriteLine(AiData.Intern_Dir);

        Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY, Position.z);
        Hitbox.Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY - _negativeHeight, Position.z);
    }

    public string WriteBehaviorList()
    {
        var compiler = new AIPropertiesCompiler();

        var bList = new SeparatedStringBuilder('`');

        SeparatedStringBuilder bDefinesList;

        foreach (var behavior in BehaviorList.BehaviorData)
        {
            bDefinesList = new SeparatedStringBuilder('|');
            bDefinesList.Append(behavior.Key);
            bDefinesList.Append(compiler.CreateBehaviorString(this, behavior.Key));
            bDefinesList.Append(compiler.CreateResources(behavior.Value.Resources));
            bList.Append(bDefinesList.ToString());
        }

        return bList.ToString();
    }

    public virtual void Damage(int damage, Player origin)
    {
        Health -= damage;

        var damageEvent = new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health, damage, 0, 0, origin == null ? string.Empty : origin.CharacterName, false, true);
        Room.SendSyncEvent(damageEvent);

        if (Health <= 0)
        {
            if (EnemyController.OnDeathTargetID is not null and not "0")
                foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(EnemyController.OnDeathTargetID))
                    trigger.Trigger(true);

            //Temp values for now
            Room.SendSyncEvent(SyncBuilder.AIDie(Entity, "PF_SFX_UI_Buy", 10, true, origin == null ? "0" : origin.GameObjectId, false));
            Destroy(origin, Room, Id);
        }
    }

    public virtual AIBaseBehavior ChangeBehavior(string behaviorName)
    {
        AiBehavior = new AIBaseBehavior();

        switch (behaviorName)
        {
            case "Patrol":
                AiBehavior = new AIBehaviorPatrol(
                    Generic.Patrol_DistanceX,
                    Generic.Patrol_DistanceY,
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Patrol", "speed")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Patrol", "endPathWaitTime")),
                    Generic.Patrol_ForceDirectionX,
                    Generic.Patrol_InitialProgressRatio
                    );
                break;

            case "Aggro":
                AiBehavior = new AIBehaviorAggro(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "speed")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "moveBeyondTargetDistance")),
                    Convert.ToBoolean(BehaviorList.GetBehaviorStat("Aggro", "stayOnPatrolPath")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "attackBeyondPatrolLine")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "detectionRangeUpY")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Aggro", "detectionRangeDownY"))
                    );
                break;

            case "LookAround":
                AiBehavior = new AIBehaviorLookAround(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("LookAround", "lookTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("LookAround", "initialProgressRatio")),
                    Convert.ToBoolean(BehaviorList.GetBehaviorStat("LookAround", "snapOnGround"))
                    );
                break;

            case "ComeBack":
                AiBehavior = new AIBehaviorComeBack(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("ComeBack", "speed"))
                    );
                AiBehavior.Start(ref AiData, Room.Time, [Position.x.ToString(), AiData.Intern_SpawnPosY.ToString()]);
                return AiBehavior;

            case "Shooting":
                AiBehavior = new AIBehaviorShooting(
                    Convert.ToInt32(BehaviorList.GetBehaviorStat("Shooting", "nbBulletsPerRound")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "fireSpreadAngle")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "delayBetweenBullet")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "delayShoot_Anim")),
                    Convert.ToInt32(BehaviorList.GetBehaviorStat("Shooting", "nbFireRounds")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "delayBetweenFireRound")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "startCoolDownTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "endCoolDownTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "projectileSpeed")),
                    Convert.ToBoolean(BehaviorList.GetBehaviorStat("Shooting", "fireSpreadClockwise")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Shooting", "fireSpreadStartAngle"))
                    );
                break;

            case "Bomber":
                AiBehavior = new AIBehaviorBomber(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Bomber", "inTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Bomber", "loopTime"))
                    );
                break;

            case "Grenadier":
                AiBehavior = new AIBehaviorGrenadier(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Grenadier", "inTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Grenadier", "loopTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Grenadier", "outTime")),
                    Convert.ToBoolean(BehaviorList.GetBehaviorStat("Grenadier", "isTracking")),
                    Convert.ToInt32(BehaviorList.GetBehaviorStat("Grenadier", "projCount")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Grenadier", "projSpeed")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Grenadier", "maxHeight"))
                    );
                break;

            case "Stomper":
                AiBehavior = new AIBehaviorStomper(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stomper", "attackTime")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stomper", "impactTime"))
                    );
                break;

            case "Stinger":
                AiBehavior = new AIBehaviorStinger(
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stinger", "speedForward")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stinger", "speedBackward")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stinger", "inDurationForward")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stinger", "attackDuration")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stinger", "damageAttackTimeOffset")),
                    Convert.ToSingle(BehaviorList.GetBehaviorStat("Stinger", "inDurationBackward"))
                    );
                AiBehavior.Start(ref AiData, Room.Time,
                    [
                        AiData.Sync_TargetPosX.ToString(),
                        AiData.Sync_TargetPosY.ToString(),
                        "0",
                        AiData.Intern_SpawnPosX.ToString(),
                        AiData.Intern_SpawnPosY.ToString(),
                        AiData.Intern_SpawnPosZ.ToString(),
                        Convert.ToString(BehaviorList.GetBehaviorStat("Stinger", "speedForward")),
                        Convert.ToString(BehaviorList.GetBehaviorStat("Stinger", "speedBackward"))
                    ]);
                return AiBehavior;
        }

        AiBehavior.Start(ref AiData, Room.Time, []);
        return AiBehavior;
    }

    public virtual void DetectPlayers(string behaviorToRun)
    {
    }

    public bool PlayerInRange(Vector3Model pos, bool limitedByPatrolLine)
    {
        if (AiData.Intern_Dir < 0)
        {
            return !limitedByPatrolLine
                ? AiData.Sync_PosX - EnemyGlobalProps.Global_FrontDetectionRangeX < pos.X && pos.X < AiData.Sync_PosX + EnemyGlobalProps.Global_BackDetectionRangeX &&
                   AiData.Sync_PosY - EnemyGlobalProps.Global_FrontDetectionRangeDownY < pos.Y && pos.Y < AiData.Sync_PosY + EnemyGlobalProps.Global_FrontDetectionRangeUpY &&
                   Position.z == pos.Z
                : AiData.Sync_PosX - EnemyGlobalProps.Global_FrontDetectionRangeX < pos.X && pos.X < AiData.Sync_PosX + EnemyGlobalProps.Global_BackDetectionRangeX &&
                   AiData.Sync_PosY - EnemyGlobalProps.Global_FrontDetectionRangeDownY < pos.Y && pos.Y < AiData.Sync_PosY + EnemyGlobalProps.Global_FrontDetectionRangeUpY &&
                   Position.z == pos.Z &&
                   pos.X > AiData.Intern_MinPointX - 1.5 && pos.X < AiData.Intern_MaxPointX + 1.5;
        }
        else if (AiData.Intern_Dir >= 0)
        {
            return !limitedByPatrolLine
                ? AiData.Sync_PosX - EnemyGlobalProps.Global_BackDetectionRangeX < pos.X && pos.X < AiData.Sync_PosX + EnemyGlobalProps.Global_FrontDetectionRangeX &&
                   AiData.Sync_PosY - EnemyGlobalProps.Global_FrontDetectionRangeDownY < pos.Y && pos.Y < AiData.Sync_PosY + EnemyGlobalProps.Global_FrontDetectionRangeUpY &&
                   Position.z == pos.Z
                : AiData.Sync_PosX - EnemyGlobalProps.Global_BackDetectionRangeX < pos.X && pos.X < AiData.Sync_PosX + EnemyGlobalProps.Global_FrontDetectionRangeX &&
                   AiData.Sync_PosY - EnemyGlobalProps.Global_FrontDetectionRangeDownY < pos.Y && pos.Y < AiData.Sync_PosY + EnemyGlobalProps.Global_FrontDetectionRangeUpY &&
                   Position.z == pos.Z &&
                   pos.X > AiData.Intern_MinPointX - 1.5 && pos.X < AiData.Intern_MaxPointX + 1.5;
        }
        return false;
    }

    public float ResetBehaviorTime(float behaviorEndTime) => Room.Time + behaviorEndTime;

    public virtual void HandleLookAround() { }

    public virtual void HandlePatrol() => AiBehavior.Update(ref AiData, Room.Time);

    public virtual void HandleComeBack() { }

    public virtual void HandleAggro() { }

    public virtual void HandleShooting()
    {
        if (AiData.Intern_FireProjectile)
        {
            var pos = new Vector3Model { X = Position.x + EnemyGlobalProps.Global_ShootOffsetX, Y = Position.y + EnemyGlobalProps.Global_ShootOffsetY, Z = Position.z };

            var rand = new System.Random();
            var prjId = Math.Abs(rand.Next()).ToString();

            while (Room.GameObjectIds.Contains(prjId))
                prjId = Math.Abs(rand.Next()).ToString();

            // Magic numbers here are temporary
            Room.SendSyncEvent(SyncBuilder.AILaunchItem(Entity, pos.X, pos.Y, pos.Z, (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, 3, int.Parse(prjId), 0));

            AiData.Intern_FireProjectile = false;

            var prj = new AIProjectileEntity(Room, Id, prjId, pos, (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, 3);
            Room.Projectiles.Add(prjId, prj);
        }
    }

    public virtual void HandleProjectile() { }

    public virtual void HandleBomber() { }

    public virtual void HandleGrenadier()
    {
        if (AiData.Intern_FireProjectile)
        {
            // Magic numbers here are temporary
            Room.SendSyncEvent(SyncBuilder.AILaunchItem(Entity, Position.x + EnemyGlobalProps.Global_ShootOffsetX, Position.y + EnemyGlobalProps.Global_ShootOffsetY, Position.z, (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, 3, 0, 1));

            AiData.Intern_FireProjectile = false;
        }
    }

    public virtual void HandleStomper() { }

    public virtual void HandleIdle() { }

    public virtual void HandleStinger() { }

    public virtual void HandleSpike() { }

    //This one is not in the helper because it needs too many arguments and too much reformatted data to quantify being there.
    public AIInit_SyncEvent AIInit(float healthMod, float sclMod, float resMod)
    {
        var aiInit = new AIInit_SyncEvent(Id, Room.Time, Position.x, Position.y, Position.z, Position.x, Position.y, Generic.Patrol_InitialProgressRatio,
        Status.MaxHealth, Status.MaxHealth, healthMod, sclMod, resMod, Status.Stars, EnemyController.Level, EnemyGlobalProps.ToString(), WriteBehaviorList());

        aiInit.EventDataList[2] = Position.x;
        aiInit.EventDataList[3] = Position.y;
        aiInit.EventDataList[4] = Position.z;

        return aiInit;
    }

    public void GetInitEnemyData(Player player)
    {
        var aiInit = new AIInit_SyncEvent(Id, Room.Time, AiData.Sync_PosX, AiData.Sync_PosY, AiData.Sync_PosZ, Position.z, AiData.Intern_SpawnPosY, Generic.Patrol_InitialProgressRatio,
        Health, Status.MaxHealth, 1f, 1f, 1f, Status.Stars, EnemyController.Level, EnemyGlobalProps.ToString(), WriteBehaviorList());

        aiInit.EventDataList[2] = AiData.Intern_SpawnPosX;
        aiInit.EventDataList[3] = AiData.Intern_SpawnPosY;
        aiInit.EventDataList[4] = Position.z;

        player.SendSyncEventToPlayer(aiInit);

        var aiDo = new AIDo_SyncEvent(new SyncEvent(Id, SyncEvent.EventType.AIDo, Room.Time));

        aiDo.EventDataList.Clear();
        aiDo.EventDataList.Add(AiData.Sync_PosX);
        aiDo.EventDataList.Add(AiData.Sync_PosY);
        aiDo.EventDataList.Add(1f);
        aiDo.EventDataList.Add(BehaviorList.IndexOf("Patrol"));
        aiDo.EventDataList.Add("");
        aiDo.EventDataList.Add(AiData.Sync_TargetPosX);
        aiDo.EventDataList.Add(AiData.Sync_TargetPosY);
        aiDo.EventDataList.Add(AiData.Intern_Dir);
        aiDo.EventDataList.Add(0);

        player.SendSyncEventToPlayer(aiDo);
    }

    public void Destroy(Player player, Room room, string id)
    {
        room.RemoveEntity(id);
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);

        player.CheckObjective(ObjectiveEnum.Score, id, Entity.PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Scoremultiple, id, Entity.PrefabName, 1);

        player.CheckAchievement(AchConditionType.DefeatEnemy, string.Empty, Logger);
        player.CheckAchievement(AchConditionType.DefeatEnemy, Entity.PrefabName, Logger);
        player.CheckAchievement(AchConditionType.DefeatEnemyInLevel, player.Room.LevelInfo.Name, Logger);
    }
}
