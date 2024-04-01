using A2m.Server;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies;

public abstract class BehaviorEnemy(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : Enemy(room, entityId, prefabName, enemyController, services), IDestructible
{
    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public AIBaseBehavior AiBehavior;

    public float BehaviorEndTime;
    public StateTypes OffensiveBehavior;

    public override void Initialize()
    {
        base.Initialize();

        // Set up base config that all behaviours have
        BehaviorEndTime = 0;
        MinBehaviorTime = Convert.ToSingle(BehaviorList.GetGlobalProperty("MinBehaviorTime"));
        OffensiveBehavior = Enum.Parse<StateTypes>(Convert.ToString(BehaviorList.GetGlobalProperty("OffensiveBehavior")));

        //External Component Info
        var global = Room.GetEntityFromId<AIStatsGlobalComp>(Id);

        if (global != null)
            Global = global;

        var generic = Room.GetEntityFromId<AIStatsGenericComp>(Id);

        if (generic != null)
            Generic = generic;

        //AIProcessData assignment, used for AI_Behavior
        AiData = new AIProcessData
        {
            Intern_SpawnPosX = Position.x,
            Intern_SpawnPosY = Position.y,
            Intern_SpawnPosZ = Position.z,
            Sync_PosX = Position.x,
            Sync_PosY = Position.y,
            SyncInit_Dir = 1,
            SyncInit_ProgressRatio = Generic.Patrol_InitialProgressRatio
        };

        AiData.SetStats(EnemyGlobalProps);
    }

    public override void CheckForSpawner()
    {
        base.CheckForSpawner();

        Global = LinkedSpawner.Global;
        Generic = LinkedSpawner.Generic;
        Status = LinkedSpawner.Status;
    }

    public override void Update()
    {
        base.Update();

        // Commented lines are behaviors that have not been added yet
        // AIBehavior_Spike
        // AIBehavior_Projectile
        // AIBehavior_Acting

        switch (AiBehavior)
        {
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
        }

        Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY, Position.z);
        Hitbox.Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY - (EnemyController.Scale.Y < 0 ? Hitbox.ColliderBox.Height : 0), Position.z);
    }

    private string WriteBehaviorList()
    {
        var bList = new SeparatedStringBuilder('`');

        foreach (var behavior in BehaviorList.BehaviorData)
        {
            var bDefinesList = new SeparatedStringBuilder('|');

            bDefinesList.Append(Enum.GetName(behavior.Key));
            bDefinesList.Append(behavior.Value.ToStateString(Generic));
            bDefinesList.Append(behavior.Value.ToResourcesString());

            bList.Append(bDefinesList.ToString());
        }

        return bList.ToString();
    }

    public virtual AIBaseBehavior ChangeBehavior(StateTypes behaviourType)
    {
        var behaviour = BehaviorList.BehaviorData[behaviourType];

        AiBehavior = behaviour.CreateBaseBehaviour(Generic);
        var args = behaviour.GetStartArgs(this);

        AiBehavior.Start(ref AiData, Room.Time, args);

        return AiBehavior;
    }

    public virtual void DetectPlayers(StateTypes stateTypes)
    {
    }

    public bool PlayerInRange(Vector3Model pos, bool limitedByPatrolLine) =>
        AiData.Sync_PosX - (AiData.Intern_Dir < 0 ? EnemyGlobalProps.Global_FrontDetectionRangeX : EnemyGlobalProps.Global_BackDetectionRangeX) < pos.X &&
            pos.X < AiData.Sync_PosX + (AiData.Intern_Dir < 0 ? EnemyGlobalProps.Global_BackDetectionRangeX : EnemyGlobalProps.Global_FrontDetectionRangeX) &&
            AiData.Sync_PosY - EnemyGlobalProps.Global_FrontDetectionRangeDownY < pos.Y && pos.Y < AiData.Sync_PosY + EnemyGlobalProps.Global_FrontDetectionRangeUpY &&
            Position.z < pos.Z + 1 && Position.z > pos.Z - 1 &&
            (!limitedByPatrolLine || pos.X > AiData.Intern_MinPointX - 1.5 && pos.X < AiData.Intern_MaxPointX + 1.5);

    public float ResetBehaviorTime(float behaviorEndTime) => Room.Time + behaviorEndTime;

    public virtual void HandleLookAround() { }

    public virtual void HandlePatrol() => AiBehavior.Update(ref AiData, Room.Time);

    public virtual void HandleComeBack() { }

    public virtual void HandleAggro() { }

    public virtual void HandleShooting()
    {
        if (AiData.Intern_FireProjectile)
        {
            var pos = new Vector3Model { X = Position.x + AiData.Intern_Dir * EnemyGlobalProps.Global_ShootOffsetX, Y = Position.y + EnemyGlobalProps.Global_ShootOffsetY, Z = Position.z };

            var rand = new System.Random();
            var projectileId = Math.Abs(rand.Next()).ToString();

            while (Room.GameObjectIds.Contains(projectileId))
                projectileId = Math.Abs(rand.Next()).ToString();

            // Magic numbers here are temporary
            Room.SendSyncEvent(
                AISyncEventHelper.AILaunchItem(Id, Room.Time, pos.X, pos.Y, pos.Z,
                    (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed,
                    (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, 3, int.Parse(projectileId), 0
                )
            );

            AiData.Intern_FireProjectile = false;

            var prj = new AIProjectile(
                Room, Id, projectileId, pos, (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed,
                (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed, 3, Room.Enemies[Id].EnemyController.TimerThread,
                GameFlow.StatisticData.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, Level),
                EnemyController.ComponentData.EnemyEffectType, ServerRConfig, ItemCatalog
            );

            Room.AddProjectile(prj);
        }
    }

    public virtual void HandleProjectile() { }

    public virtual void HandleBomber() { }

    public virtual void HandleGrenadier()
    {
        if (AiData.Intern_FireProjectile)
        {
            // Magic numbers here are temporary
            Room.SendSyncEvent(AISyncEventHelper.AILaunchItem(Id, Room.Time,
                Position.x + AiData.Intern_Dir * EnemyGlobalProps.Global_ShootOffsetX,
                Position.y + EnemyGlobalProps.Global_ShootOffsetY,
                Position.z,
                (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed,
                (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed,
                3, 0, 1)
            );

            AiData.Intern_FireProjectile = false;
        }
    }

    public virtual void HandleStomper() { }

    public virtual void HandleIdle() { }

    public virtual void HandleStinger() { }

    public virtual void HandleSpike() { }

    //This one is not in the helper because it needs too many arguments and too much reformatted data to qualify being there.
    public AIInit_SyncEvent AIInit(float healthMod, float sclMod, float resMod)
    {
        var aiInit = new AIInit_SyncEvent(Id, Room.Time, Position.x, Position.y, Position.z, Position.x, Position.y, Generic.Patrol_InitialProgressRatio,
        Health, MaxHealth, healthMod, sclMod, resMod, Status.Stars, Level, EnemyGlobalProps.ToString(), WriteBehaviorList());

        aiInit.EventDataList[2] = Position.x;
        aiInit.EventDataList[3] = Position.y;
        aiInit.EventDataList[4] = Position.z;

        return aiInit;
    }

    public override void GetInitData(Player player)
    {
        var aiInit = new AIInit_SyncEvent(Id, Room.Time, AiData.Sync_PosX, AiData.Sync_PosY, AiData.Sync_PosZ, AiData.Intern_SpawnPosX, AiData.Intern_SpawnPosY, AiBehavior.GetBehaviorRatio(ref AiData, Room.Time),
        Health, MaxHealth, 1f, 1f, 1f, Status.Stars, Level, EnemyGlobalProps.ToString(), WriteBehaviorList());

        aiInit.EventDataList[2] = AiData.Intern_SpawnPosX;
        aiInit.EventDataList[3] = AiData.Intern_SpawnPosY;
        aiInit.EventDataList[4] = Position.z;

        player.SendSyncEventToPlayer(aiInit);

        var aiDo = new AIDo_SyncEvent(new SyncEvent(Id, SyncEvent.EventType.AIDo, Room.Time));

        aiDo.EventDataList.Add(AiData.Sync_PosX);
        aiDo.EventDataList.Add(AiData.Sync_PosY);
        aiDo.EventDataList.Add(1f);
        aiDo.EventDataList.Add(GetIndexOfCurrentBehavior());
        aiDo.EventDataList.Add(string.Empty);
        aiDo.EventDataList.Add(AiData.Sync_TargetPosX);
        aiDo.EventDataList.Add(AiData.Sync_TargetPosY);
        aiDo.EventDataList.Add(AiData.Intern_Dir);
        aiDo.EventDataList.Add(0);

        player.SendSyncEventToPlayer(aiDo);
    }

    public int GetIndexOfCurrentBehavior() => BehaviorList.IndexOf(AiBehavior.GetBehavior());
}
