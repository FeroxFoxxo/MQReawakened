using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Entities.Enemies.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes;

public class BehaviorEnemy(EnemyData data) : BaseEnemy(data)
{
    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public AIBaseBehavior AiBehavior;

    public float HealthModifier;
    public float ScaleModifier;
    public float ResistanceModifier;

    private object _enemyLock;

    private StateType _currentState;
    private float _lastUpdate;
    private int _index;

    public TimerThread TimerThread;

    public override void Initialize()
    {
        _enemyLock = new object();
        TimerThread = Services.GetRequiredService<TimerThread>();

        Global = Room.GetEntityFromId<AIStatsGlobalComp>(Id);
        Generic = Room.GetEntityFromId<AIStatsGenericComp>(Id);

        var classCopier = Services.GetRequiredService<ClassCopier>();

        GlobalProperties = EnemyModel.GlobalProperties.GenerateGlobalPropertiesFromModel(classCopier, Global);
        GenericScript = EnemyModel.GenericScript.GenerateGenericPropertiesFromModel(classCopier, Global);

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
            _shoot = new Shooter(this),
            _bomber = new Bomber(this),
            _scan = new Scanner(),
            _collision = new Collisions(),
            _suicide = new Runnable()
        };

        // Address magic numbers when we get to adding enemy effect mods
        SendAiInit(1, 1, 1);

        base.Initialize();
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

    public bool PlayerInRange(Vector3 pos, bool limitedByPatrolLine) =>
        AiData.Sync_PosX - (AiData.Intern_Dir < 0 ? GlobalProperties.Global_FrontDetectionRangeX : GlobalProperties.Global_BackDetectionRangeX) < pos.x &&
            pos.x < AiData.Sync_PosX + (AiData.Intern_Dir < 0 ? GlobalProperties.Global_BackDetectionRangeX : GlobalProperties.Global_FrontDetectionRangeX) &&
            AiData.Sync_PosY - GlobalProperties.Global_FrontDetectionRangeDownY < pos.y && pos.y < AiData.Sync_PosY + GlobalProperties.Global_FrontDetectionRangeUpY &&
            Position.z < pos.z + 1 && Position.z > pos.z - 1 &&
            (!limitedByPatrolLine || pos.x > AiData.Intern_MinPointX - 1.5 && pos.x < AiData.Intern_MaxPointX + 1.5);

    public override void InternalUpdate()
    {
        var hasDetected = false;

        if (AiBehavior.ShouldDetectPlayers)
            hasDetected = HasDetectedPlayers();

        if (!hasDetected)
        {
            if (AiBehavior.TryUpdate(AiData, Room.Time, this))
                if (AiData.Intern_FireProjectile)
                    FireProjectile(false);

            if (_currentState == GenericScript.AwareBehavior)
                if (Room.Time >= _lastUpdate + GenericScript.genericScript_AwareBehaviorDuration)
                    AiBehavior.NextState(this);
        }

        Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY, Position.z);

        Hitbox.Position = new Vector3(
            AiData.Sync_PosX,
            AiData.Sync_PosY - (EnemyController.Scale.Y < 0 ? Hitbox.ColliderBox.Height : 0),
            Position.z
        );
    }

    public bool HasDetectedPlayers()
    {
        foreach (var player in Room.GetPlayers())
        {
            if (PlayerInRange(player.TempData.Position, GlobalProperties.Global_DetectionLimitedByPatrolLine))
            {
                AiData.Sync_TargetPosX = player.TempData.Position.x;

                AiData.Sync_TargetPosY = GenericScript.AttackBehavior == StateType.Grenadier ?
                    Position.y : player.TempData.Position.y;

                ChangeBehavior(
                    GenericScript.AttackBehavior,
                    player.TempData.Position.x,
                    GenericScript.AttackBehavior == StateType.Grenadier ? player.TempData.Position.y : Position.y,
                    Generic.Patrol_ForceDirectionX
                );

                return true;
            }
        }

        return false;
    }

    public void FireProjectile(bool isGrenade)
    {
        var position = new Vector3
        {
            x = Position.x + AiData.Intern_Dir * GlobalProperties.Global_ShootOffsetX,
            y = Position.y + GlobalProperties.Global_ShootOffsetY,
            z = Position.z
        };

        var speed = new Vector2
        {
            x = (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed,
            y = (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed
        };

        FireProjectile(position, speed, isGrenade);

        AiData.Intern_FireProjectile = false;
    }

    public void FireProjectile(Vector3 position, Vector2 speed, bool isGrenade) =>
        Room.AddRangedProjectile(Id, position, speed, 3, GetDamage(), EnemyController.ComponentData.EnemyEffectType, isGrenade);

    public int GetDamage() =>
        GameFlow.StatisticData.GetValue(
            ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, Level
        );

    public void ChangeBehavior(StateType behaviourType, float targetX, float targetY, int direction)
    {
        lock (_enemyLock)
        {
            if (AiData != null && AiBehavior != null)
                AiBehavior.Stop(AiData);

            _currentState = behaviourType;
            _index = EnemyModel.IndexOf(behaviourType);
            _lastUpdate = Room.Time;

            var behaviour = EnemyModel.BehaviorData[behaviourType];

            AiBehavior = behaviour.GetBaseBehaviour(Global, Generic);

            var args = behaviour.GetStartArgs(this);

            AiBehavior.Start(AiData, Room.Time, args);

            Room.SendSyncEvent(
                AISyncEventHelper.AIDo(
                    Id, Room.Time, Position.x, Position.y, 1.0f,
                    _index, args, targetX, targetY, direction, false
                )
            );
        }
    }

    public override void Damage(int damage, Player player)
    {
        base.Damage(damage, player);

        if (player == null)
        {
            Logger.LogError("Could not find player that damaged {PrefabName}! Returning...", PrefabName);
            return;
        }

        if (_currentState != StateType.Shooting)
        {
            AiData.Sync_TargetPosX = player.TempData.Position.x;
            AiData.Sync_TargetPosY = player.TempData.Position.y;

            ChangeBehavior(
                GenericScript.AttackBehavior,
                player.TempData.Position.x, player.TempData.Position.y,
                Generic.Patrol_ForceDirectionX
            );
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
                AiBehavior.GetBehaviorRatio(AiData, Room.Time)
            )
        );

        player.SendSyncEventToPlayer(
            AISyncEventHelper.AIDo(
                Id, Room.Time, AiData.Sync_PosX, AiData.Sync_PosY,
                1.0f, _index, AiBehavior.GetInitArgs(),
                AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir, false
            )
        );
    }

    public AIInit_SyncEvent GetEnemyInit(float poxX, float posY, float posZ,
        float spawnX, float spawnY, float behaviourRatio) =>
            AISyncEventHelper.AIInit(
                Id, Room.Time, poxX, posY, posZ, spawnX, spawnY, behaviourRatio,
                Health, MaxHealth, HealthModifier, ScaleModifier, ResistanceModifier,
                Status.Stars, Level, GlobalProperties, EnemyModel.BehaviorData, Global, Generic
            );
}
