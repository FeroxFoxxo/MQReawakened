using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Colliders;
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

        AiData.SetStats(Global.GetGlobalProperties());

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

            if (CurrentState == Global.AwareBehavior || CurrentState == StateType.LookAround)
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
        var rect = new Rect(
            Hitbox.Position.x - (AiData.Intern_Dir < 0 ? Global.Global_FrontDetectionRangeX : Global.Global_BackDetectionRangeX) - Hitbox.BoundingBox.width / 2,
            Hitbox.Position.y - (AiData.Intern_Dir < 0 ? Global.Global_FrontDetectionRangeDownY : Global.Global_BackDetectionRangeDownY) - Hitbox.BoundingBox.height / 2,
            Global.Global_FrontDetectionRangeX + Global.Global_BackDetectionRangeX + Hitbox.BoundingBox.width,
            Global.Global_BackDetectionRangeDownY + Global.Global_FrontDetectionRangeDownY + Hitbox.BoundingBox.height
        );

        var enemyCollider = new EnemyCollider(Id, Vector3.zero, rect, ParentPlane, Room);

        foreach (var player in Room.GetPlayers())
        {
            if (
                enemyCollider.CheckCollision(new PlayerCollider(player)) &&
                (!Global.Global_DetectionLimitedByPatrolLine || player.TempData.Position.x > AiData.Intern_MinPointX && player.TempData.Position.x < AiData.Intern_MaxPointX) &&
                ParentPlane == player.GetPlayersPlaneString() && !player.Character.StatusEffects.HasEffect(ItemEffectType.Invisibility) &&
                player.Character.CurrentLife > 0)
            {
                EnemyAggroPlayer(player);

                return true;
            }
        }

        return false;
    }

    public void FireProjectile(bool isGrenade)
    {
        var position = new Vector3
        {
            x = Position.x + AiData.Intern_Dir * Global.Global_ShootOffsetX,
            y = Position.y + Global.Global_ShootOffsetY,
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
                    targetX, targetY, direction, CurrentState == Global.AwareBehavior,
                    this
                )
            );
        }
    }

    public override void Damage(Player player, int damage)
    {
        base.Damage(player, damage);
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
                AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir, CurrentState == Global.AwareBehavior,
                this
            )
        );
    }

    public void EnemyAggroPlayer(Player player)
    {
        if (player == null)
        {
            Logger.LogError("Could not find player that damaged {PrefabName}! Returning...", PrefabName);
            return;
        }

        AiData.Sync_TargetPosX = player.TempData.Position.x;
        AiData.Sync_TargetPosY = player.TempData.Position.y;

        ChangeBehavior(
            Global.AttackBehavior,
            player.TempData.Position.x, player.TempData.Position.y,
            Generic.Patrol_ForceDirectionX
        );
    }
}
