using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;

public class BehaviorEnemy(EnemyData data) : Enemy(data)
{
    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public AIBaseBehavior AiBehavior;

    public float BehaviorEndTime;

    public float HealthModifier;
    public float ScaleModifier;
    public float ResistanceModifier;

    public override void Initialize()
    {
        base.Initialize();

        //External Component Info
        Global = Room.GetEntityFromId<AIStatsGlobalComp>(Id);
        Generic = Room.GetEntityFromId<AIStatsGenericComp>(Id);

        // Set up base config that all behaviours have
        BehaviorEndTime = 0;

        var classCopier = Services.GetRequiredService<ClassCopier>();

        GlobalProperties = EnemyModel.GlobalProperties.GenerateGlobalPropertiesFromModel(classCopier, Global);
        GenericScript = EnemyModel.GenericScript.GenerateGenericPropertiesFromModel(classCopier, Global);

        //AIProcessData assignment, used for AI_Behavior
        AiData = new AIProcessData
        {
            Intern_SpawnPosX = Position.x,
            Intern_SpawnPosY = Position.y,
            Intern_SpawnPosZ = Position.z,
            Sync_PosX = Position.x,
            Sync_PosY = Position.y,
            Sync_PosZ = Position.z,
            SyncInit_Dir = 1,
            SyncInit_ProgressRatio = Generic.Patrol_InitialProgressRatio
        };

        AiData.SetStats(GlobalProperties);

        AiData.services = new AIServices
        {
            _shoot = new IShoot(),
            _bomber = new IBomber(),
            _scan = new IScan()
        };

        // Address magic numbers when we get to adding enemy effect mods
        SendAiInit(1, 1, 1);
    }

    public override void CheckForSpawner()
    {
        base.CheckForSpawner();

        // Should use apt template rather than first
        var spawnerTemplate = LinkedSpawner.TemplateEnemyModels.FirstOrDefault().Value;

        if (spawnerTemplate is null)
        {
            Logger.LogError("Spawner with {Id} has invalid templates! Returning...", LinkedSpawner.Id);
            return;
        }

        Global = spawnerTemplate.Global;
        Generic = spawnerTemplate.Generic;
        Status = spawnerTemplate.Status;
    }

    public bool PlayerInRange(Vector3Model pos, bool limitedByPatrolLine) =>
        AiData.Sync_PosX - (AiData.Intern_Dir < 0 ? GlobalProperties.Global_FrontDetectionRangeX : GlobalProperties.Global_BackDetectionRangeX) < pos.X &&
            pos.X < AiData.Sync_PosX + (AiData.Intern_Dir < 0 ? GlobalProperties.Global_BackDetectionRangeX : GlobalProperties.Global_FrontDetectionRangeX) &&
            AiData.Sync_PosY - GlobalProperties.Global_FrontDetectionRangeDownY < pos.Y && pos.Y < AiData.Sync_PosY + GlobalProperties.Global_FrontDetectionRangeUpY &&
            Position.z < pos.Z + 1 && Position.z > pos.Z - 1 &&
            (!limitedByPatrolLine || pos.X > AiData.Intern_MinPointX - 1.5 && pos.X < AiData.Intern_MaxPointX + 1.5);

    public void ResetBehaviorTime(float behaviorEndTime)
    {
        if (behaviorEndTime < AwareBehaviorDuration)
            behaviorEndTime = AwareBehaviorDuration;

        BehaviorEndTime = Room.Time + behaviorEndTime;
    }

    public override void InternalUpdate()
    {
        /* Behaviors that have not been added yet:
                AIBehavior_Spike
                AIBehavior_Projectile
                AIBehavior_Acting
        */

        switch (AiBehavior)
        {
            case AIBehaviorAggro:
                if (!AiBehavior.Update(ref AiData, Room.Time))
                {
                    ChangeBehavior(GenericScript.AwareBehavior,
                        GenericScript.UnawareBehavior == StateType.ComeBack ? Position.x : AiData.Sync_TargetPosX,
                        GenericScript.UnawareBehavior == StateType.ComeBack ? Position.y : AiData.Sync_TargetPosY,
                        AiData.Intern_Dir
                    );
                }
                break;

            case AIBehaviorLookAround:
                DetectPlayers(GenericScript.AttackBehavior);

                if (Room.Time >= BehaviorEndTime)
                {
                    ChangeBehavior(
                        GenericScript.UnawareBehavior,
                        Position.x,
                        GenericScript.UnawareBehavior == StateType.ComeBack ? AiData.Intern_SpawnPosY : Position.y,
                        Generic.Patrol_ForceDirectionX
                    );

                    AiBehavior.MustDoComeback(AiData);
                }
                break;

            case AIBehaviorPatrol:
                AiBehavior.Update(ref AiData, Room.Time);

                if (Room.Time >= BehaviorEndTime)
                    DetectPlayers(GenericScript.AttackBehavior);
                break;

            case AIBehaviorComeBack:
                if (!AiBehavior.Update(ref AiData, Room.Time))
                    ChangeBehavior(StateType.Patrol, Position.x, Position.y, Generic.Patrol_ForceDirectionX);
                break;

            case AIBehaviorShooting:
                TryFireProjectile(false);

                if (!AiBehavior.Update(ref AiData, Room.Time))
                    ChangeBehavior(GenericScript.AwareBehavior, AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir);
                break;

            case AIBehaviorBomber:
                if (Room.Time >= BehaviorEndTime)
                    Damage(EnemyController.MaxHealth, null);
                break;

            case AIBehaviorGrenadier:
                TryFireProjectile(true);

                if (Room.Time >= BehaviorEndTime)
                    ChangeBehavior(GenericScript.AwareBehavior, Position.x, Position.y, AiData.Intern_Dir);
                break;

            case AIBehaviorStomper:
                if (Room.Time >= BehaviorEndTime)
                    ChangeBehavior(GenericScript.UnawareBehavior, Position.x, Position.y, Generic.Patrol_ForceDirectionX);
                break;

            case AIBehaviorStinger:
                if (!AiBehavior.Update(ref AiData, Room.Time))
                    ChangeBehavior(GenericScript.AwareBehavior, Position.x, Position.y, AiData.Intern_Dir);
                break;

            default:
                Logger.LogError("Behavioral enemy '{Enemy}' has no update method for '{Behavior}'", PrefabName, AiBehavior.ToString());
                break;
        }

        Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY, Position.z);
        Hitbox.Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY - (EnemyController.Scale.Y < 0 ? Hitbox.ColliderBox.Height : 0), Position.z);
    }

    public void DetectPlayers(StateType type)
    {
        foreach (var player in Room.Players.Values)
        {
            if (PlayerInRange(player.TempData.Position, GlobalProperties.Global_DetectionLimitedByPatrolLine))
            {
                AiData.Sync_TargetPosX = player.TempData.Position.X;
                AiData.Sync_TargetPosY = GenericScript.AttackBehavior == StateType.Grenadier ? Position.y : player.TempData.Position.Y;

                ChangeBehavior(type, player.TempData.Position.X, GenericScript.AttackBehavior == StateType.Grenadier ? player.TempData.Position.Y : Position.y, Generic.Patrol_ForceDirectionX);
            }
        }
    }

    public void TryFireProjectile(bool isGrenade)
    {
        if (AiData.Intern_FireProjectile)
        {
            var pos = new Vector3Model {
                X = Position.x + AiData.Intern_Dir * GlobalProperties.Global_ShootOffsetX,
                Y = Position.y + GlobalProperties.Global_ShootOffsetY,
                Z = Position.z
            };

            var projectileId = GetProjectileId();

            var speedX = (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed;
            var speedY = (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed;

            Room.SendSyncEvent(
                AISyncEventHelper.AILaunchItem(
                    Id, Room.Time, pos.X, pos.Y, pos.Z, speedX, speedY, 3, projectileId, isGrenade
                )
            );

            var timerThread = Room.Enemies[Id].EnemyController.TimerThread;
            var damage = GameFlow.StatisticData.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, Level);
            var effect = EnemyController.ComponentData.EnemyEffectType;

            var prj = new AIProjectile(
                Room, Id, projectileId.ToString(), pos, speedX, speedY,
                3, timerThread, damage, effect, ServerRConfig, ItemCatalog
            );

            Room.AddProjectile(prj);
            AiData.Intern_FireProjectile = false;
        }
    }

    public int GetProjectileId()
    {
        var rand = new System.Random();

        var projectileId = Math.Abs(rand.Next());

        return Room.GameObjectIds.Contains(projectileId.ToString()) ? GetProjectileId() : projectileId;
    }

    public void ChangeBehavior(StateType behaviourType, float targetX, float targetY, int direction)
    {
        var behaviour = EnemyModel.BehaviorData[behaviourType];
        var index = EnemyModel.IndexOf(behaviourType);

        AiBehavior = behaviour.CreateBaseBehaviour(Global, Generic);

        var args = behaviour.GetStartArgs(this);

        AiBehavior.Start(ref AiData, Room.Time, args);

        Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time, Position.x, Position.y, 1.0f, index, args, targetX, targetY, direction, false));

        ResetBehaviorTime(AiBehavior.ResetTime);
    }

    public override void Damage(int damage, Player player)
    {
        base.Damage(damage, player);

        if (player == null)
        {
            Logger.LogError("Could not change behavior of {PrefabName} when damaged!", PrefabName);
            return;
        }

        if (AiBehavior is not AIBehaviorShooting)
        {
            AiData.Sync_TargetPosX = player.TempData.Position.X;
            AiData.Sync_TargetPosY = player.TempData.Position.Y;

            ChangeBehavior(GenericScript.AttackBehavior, player.TempData.Position.X, player.TempData.Position.Y, Generic.Patrol_ForceDirectionX);

            ResetBehaviorTime(AwareBehaviorDuration);
        }
    }

    public void SendAiInit(float healthMod, float sclMod, float resMod)
    {
        HealthModifier = healthMod;
        ScaleModifier = sclMod;
        ResistanceModifier = resMod;

        Room.SendSyncEvent(
            GetEnemyInit(
                Position.x, Position.y, Position.z,
                Position.x, Position.y,
                Generic.Patrol_InitialProgressRatio
            )
        );

        ChangeBehavior(StateType.Patrol, Position.x, Position.y, Generic.Patrol_ForceDirectionX);
    }

    public override void SendAiData(Player player)
    {
        player.SendSyncEventToPlayer(
            GetEnemyInit(
                AiData.Sync_PosX, AiData.Sync_PosY, AiData.Sync_PosZ,
                AiData.Intern_SpawnPosX, AiData.Intern_SpawnPosY,
                AiBehavior.GetBehaviorRatio(ref AiData, Room.Time)
            )
        );

        player.SendSyncEventToPlayer(AISyncEventHelper.AIDo(Id, Room.Time, AiData.Sync_PosX, AiData.Sync_PosY, 1.0f, GetIndexOfCurrentBehavior(), AiBehavior.GetInitArgs(), AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir, false));
    }

    public AIInit_SyncEvent GetEnemyInit(float poxX, float posY, float posZ, float spawnX, float spawnY, float behaviourRatio) =>
            AISyncEventHelper.AIInit(
                Id, Room.Time, poxX, posY, posZ, spawnX, spawnY, behaviourRatio,
                Health, MaxHealth, HealthModifier, ScaleModifier, ResistanceModifier,
                Status.Stars, Level, GlobalProperties, EnemyModel.BehaviorData, Global, Generic
            );


    public int GetIndexOfCurrentBehavior() => EnemyModel.IndexOf(AiBehavior.GetBehavior());
}
