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
using Server.Reawakened.XMLs.Data.Enemy.Models;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes;

public class BehaviorEnemy(EnemyData data) : BaseEnemy(data)
{
    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public AIProcessData AiData;

    public GenericScriptPropertiesModel GenericScript;

    public Dictionary<StateType, AIBaseBehavior> Behaviors;

    public StateType CurrentState;
    public AIBaseBehavior CurrentBehavior;

    private object _enemyLock;
    private float _lastUpdate;

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
            SyncInit_Dir = 0,
            SyncInit_ProgressRatio = Generic.Patrol_InitialProgressRatio
        };

        AiData.SetStats(GlobalProperties);

        AiData.services = new AIServices
        {
            _shoot = new Shooter(this),
            _bomber = new Bomber(this),
            _scan = new Scanner(this),
            _collision = new Collisions(this),
            _suicide = new Runnable(this)
        };

        Behaviors = EnemyModel.BehaviorData.ToDictionary(s => s.Key, s => s.Value.GetBaseBehaviour(this));

        Room.SendSyncEvent(
            AISyncEventHelper.AIInit(
                Position.x, Position.y, Position.z,
                Position.x, Position.y,
                Generic.Patrol_InitialProgressRatio, this
            )
        );

        ChangeBehavior(StateType.Patrol, Position.x, Position.y, Generic.Patrol_ForceDirectionX);

        base.Initialize();
    }

    public override void CheckForSpawner()
    {
        base.CheckForSpawner();

        if (LinkedSpawner == null)
            return;

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

    public bool PlayerInRange(Vector3 pos, bool limitedByPatrolLine)
    {
        var originBounds = new Vector3
        (
            AiData.Sync_PosX - (AiData.Intern_Dir < 0 ? GlobalProperties.Global_FrontDetectionRangeX : GlobalProperties.Global_BackDetectionRangeX),
            AiData.Sync_PosY - GlobalProperties.Global_FrontDetectionRangeDownY,
            Position.z - 1
        );

        var maxBounds = new Vector3
        (
            AiData.Sync_PosX + (AiData.Intern_Dir < 0 ? GlobalProperties.Global_BackDetectionRangeX : GlobalProperties.Global_FrontDetectionRangeX),
            AiData.Sync_PosY + GlobalProperties.Global_FrontDetectionRangeUpY,
            Position.z + 1
        );

        return pos.x > originBounds.x && pos.x < maxBounds.x &&
            pos.y > originBounds.y && pos.y < maxBounds.y &&
            (!limitedByPatrolLine || pos.x > AiData.Intern_MinPointX && pos.x < AiData.Intern_MaxPointX);
    }

    public override void InternalUpdate()
    {
        var hasDetected = false;

        if (CurrentBehavior.ShouldDetectPlayers)
            hasDetected = HasDetectedPlayers();

        if (!hasDetected)
        {
            if (CurrentBehavior.TryUpdate())
                if (AiData.Intern_FireProjectile)
                    FireProjectile(false);

            if (CurrentState == GenericScript.AwareBehavior || CurrentState == StateType.LookAround)
                if (Room.Time >= _lastUpdate + CurrentBehavior.GetBehaviorTime())
                    CurrentBehavior.NextState();
        }

        Position = new Vector3(AiData.Sync_PosX, AiData.Sync_PosY, Position.z);

        Hitbox.Position = new Vector3(
            AiData.Sync_PosX,
            AiData.Sync_PosY - (EnemyController.Scale.Y < 0 ? Hitbox.BoundingBox.height : 0),
            Position.z
        );
    }

    public bool HasDetectedPlayers()
    {
        foreach (var player in Room.GetPlayers())
        {
            player.Character.StatusEffects.Get(ItemEffectType.Invisibility);
            if (PlayerInRange(player.TempData.Position, GlobalProperties.Global_DetectionLimitedByPatrolLine) &&
                ParentPlane == player.GetPlayersPlaneString() && !player.Character.StatusEffects.Effects.ContainsKey(ItemEffectType.Invisibility) && 
                player.Character.CurrentLife > 0)
            {
                AiData.Sync_TargetPosX = player.TempData.Position.x;
                AiData.Sync_TargetPosY = player.TempData.Position.y;

                ChangeBehavior(
                    GenericScript.AttackBehavior,
                    AiData.Sync_TargetPosX,
                    AiData.Sync_TargetPosY,
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
        Room.AddRangedProjectile(Id, position, speed, 3, GetDamage(), EnemyController.EnemyEffectType, isGrenade);

    public int GetDamage() =>
        GameFlow.StatisticData.GetValue(
            ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, Level
        );

    public void ChangeBehavior(StateType behaviourType, float targetX, float targetY, int direction)
    {
        lock (_enemyLock)
        {
            // Syncs direction of client entity with server
            if (direction == 0)
                direction = AiData.Intern_Dir;

            if (AiData.Intern_PendingSpeedFactor >= 0f)
            {
                AiData.Sync_SpeedFactor = AiData.Intern_PendingSpeedFactor;
                AiData.Intern_AnimSpeed = AiData.Intern_PendingSpeedFactor;
                AiData.Intern_PendingSpeedFactor = -1f;
            }

            CurrentBehavior?.Stop();

            CurrentBehavior = Behaviors[behaviourType];
            CurrentState = behaviourType;

            Behaviors[behaviourType].Start();

            _lastUpdate = Room.Time;

            Room.SendSyncEvent(
                AISyncEventHelper.AIDo(
                    Position.x, Position.y, 1.0f,
                    targetX, targetY, direction, CurrentState == GenericScript.AwareBehavior,
                    this
                )
            );
        }
    }

    public override void Damage(Player player, int damage)
    {
        base.Damage(player, damage);

        if (player == null)
        {
            Logger.LogError("Could not find player that damaged {PrefabName}! Returning...", PrefabName);
            return;
        }

        if (CurrentState != StateType.Shooting)
        {
            AiData.Sync_TargetPosX = player.TempData.Position.x;
            AiData.Sync_TargetPosY = player.TempData.Position.y;

            ChangeBehavior(
                GenericScript.AttackBehavior,
                player.TempData.Position.x, player.TempData.Position.y,
                Generic.Patrol_ForceDirectionX
            );
        }
        EnemyAggroPlayer(player);
    }

    public override void PetDamage(Player player)
    {
        base.PetDamage(player);
        EnemyAggroPlayer(player);
    }

    public override void SendAiData(Player player)
    {
        player.SendSyncEventToPlayer(
            AISyncEventHelper.AIInit(
                AiData.Sync_PosX, AiData.Sync_PosY, AiData.Sync_PosZ,
                AiData.Intern_SpawnPosX, AiData.Intern_SpawnPosY,
                CurrentBehavior.GetBehaviorRatio(Room.Time), this
            )
        );

        player.SendSyncEventToPlayer(
            AISyncEventHelper.AIDo(
                AiData.Sync_PosX, AiData.Sync_PosY, 1.0f,
                AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir, CurrentState == GenericScript.AwareBehavior,
                this
            )
        );
    }

    public void EnemyAggroPlayer(Player player)
    {
        if (CurrentState != StateType.Shooting)
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
}
