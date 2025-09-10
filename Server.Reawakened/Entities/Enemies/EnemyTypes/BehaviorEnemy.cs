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

    private float _lastUpdate;

    public TimerThread TimerThread;

    public override void Initialize()
    {
        TimerThread = Services.GetRequiredService<TimerThread>();

        Global = Room.GetEntityFromId<AIStatsGlobalComp>(Id);
        Generic = Room.GetEntityFromId<AIStatsGenericComp>(Id);

        AiData = new AIProcessData
        {
            Intern_SpawnPosX = Position.X,
            Intern_SpawnPosY = Position.Y,
            Intern_SpawnPosZ = Position.Z,
            Sync_PosX = Position.X,
            Sync_PosY = Position.Y,
            Sync_PosZ = Position.Z,
            SyncInit_Dir = 0,
            SyncInit_ProgressRatio = Generic?.Patrol_InitialProgressRatio ?? 0f
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

        base.Initialize();
        
        Room.SendSyncEvent(
            AISyncEventHelper.AIInit(
                Position.X, Position.Y, Position.Z,
                Position.X, Position.Y,
                Generic?.Patrol_InitialProgressRatio ?? 0f, this
            )
        );

        ChangeBehavior(StateType.Patrol, Position.X, Position.Y, Generic?.Patrol_ForceDirectionX ?? 0);
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

            if (Global != null && (CurrentState == Global.AwareBehavior || CurrentState == StateType.LookAround))
                if (Room.Time >= _lastUpdate + CurrentBehavior.GetBehaviorTime())
                    CurrentBehavior.NextState();
        }

        Position.SetPosition(
            AiData.Sync_PosX,
            AiData.Sync_PosY,
            AiData.Sync_PosZ
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

        var enemyCollider = new EnemyCollider(this, Hitbox.Position, rect, ParentPlane, Room, true);

        foreach (var player in Room.GetPlayers())
        {
            if (player == null)
                continue;

            var temp = player.TempData;
            var character = player.Character;
            var statusEffects = character?.StatusEffects;

            var collides = player.TempData.PlayerCollider != null && enemyCollider.CheckCollision(player.TempData.PlayerCollider);
            var withinPatrol = !Global.Global_DetectionLimitedByPatrolLine || temp != null && temp.Position.x > AiData.Intern_MinPointX && temp.Position.x < AiData.Intern_MaxPointX;
            var samePlane = ParentPlane == player.GetPlayersPlaneString();
            var invisible = statusEffects?.HasEffect(ItemEffectType.Invisibility) ?? false;
            var alive = (character?.CurrentLife ?? 0) > 0;

            if (collides && withinPatrol && samePlane && !invisible && alive)
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
            x = Position.X + AiData.Intern_Dir * Global.Global_ShootOffsetX,
            y = Position.Y + Global.Global_ShootOffsetY,
            z = Position.Z
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
                Position.X, Position.Y, 1.0f,
                targetX, targetY, direction, Global != null && CurrentState == Global.AwareBehavior,
                this
            )
        );
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
                AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir, Global != null && CurrentState == Global.AwareBehavior,
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
