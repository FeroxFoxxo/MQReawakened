using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;
using Server.Reawakened.Rooms;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Microsoft.Extensions.DependencyInjection;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Players;
using A2m.Server;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Models.Enemy.Models;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Rooms.Models.Entities.Colliders;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Entities.Components.Misc;
using Server.Reawakened.Entities.Components.GameObjects.Controllers;
using Server.Reawakened.Entities.Components.GameObjects.Breakables.Interfaces;

namespace Server.Reawakened.Entities.Enemies;

public abstract class Enemy : IDestructible
{
    public readonly ILogger<BehaviorEnemy> Logger;
    public readonly InternalAchievement InternalAchievement;
    public readonly QuestCatalog QuestCatalog;
    public readonly ItemCatalog ItemCatalog;
    public readonly InternalEnemyData InternalEnemy;
    public readonly ServerRConfig ServerRConfig;

    public readonly Room Room;

    public bool Init;

    public string Id;
    public Vector3 Position;
    public Rect DetectionRange;
    public EnemyCollider Hitbox;
    public string ParentPlane;
    public bool IsFromSpawner;
    public float AwareBehaviorDuration;

    public readonly string PrefabName;

    public int Health;
    public int MaxHealth;
    public int Level;
    public int DeathXp;
    public string OnDeathTargetId;

    public BaseSpawnerControllerComp LinkedSpawner;
    public InterObjStatusComp Status;
    public AIProcessData AiData;

    public GlobalProperties GlobalProperties;
    public GenericScriptPropertiesModel GenericScript;

    public readonly BaseComponent Entity;
    public readonly EnemyControllerComp EnemyController;
    public readonly EnemyModel EnemyModel;

    public readonly AISyncEventHelper SyncBuilder;

    protected IServiceProvider Services;

    public Enemy(EnemyData data)
    {
        Room = data.Room;
        Id = data.EntityId;
        PrefabName = data.PrefabName;
        Services = data.Services;
        EnemyController = data.EnemyController;
        EnemyModel = data.EnemyModel;

        IsFromSpawner = false;
        AwareBehaviorDuration = 0;
        SyncBuilder = new AISyncEventHelper();

        Logger = Services.GetRequiredService<ILogger<BehaviorEnemy>>();
        InternalAchievement = Services.GetRequiredService<InternalAchievement>();
        QuestCatalog = Services.GetRequiredService<QuestCatalog>();
        ItemCatalog = Services.GetRequiredService<ItemCatalog>();
        ServerRConfig = Services.GetRequiredService<ServerRConfig>();

        ParentPlane = EnemyController.ParentPlane;
        Position = new Vector3(EnemyController.Position.X, EnemyController.Position.Y, EnemyController.Position.Z);
        
        Status = Room.GetEntityFromId<InterObjStatusComp>(Id);

        switch (ParentPlane)
        {
            case "TemplatePlane":
                CheckForSpawner();
                break;
            case "Plane1":
                Position.z = 20;
                break;
            case "Plane0":
                Position.z = 0;
                break;
            default:
                Logger.LogError("Unknown plane: '{Plane}' for enemy {Name}", ParentPlane, PrefabName);
                break;
        }

        OnDeathTargetId = EnemyController.OnDeathTargetID;
        Health = EnemyController.EnemyHealth;
        MaxHealth = EnemyController.MaxHealth;
        DeathXp = EnemyController.OnKillExp;
        Level = EnemyController.Level;

        GenerateHitbox(EnemyModel.Hitbox);

        GlobalProperties = AISyncEventHelper.CreateDefaultGlobalProperties();
        GenericScript = AISyncEventHelper.CreateDefaultGenericScript();
    }

    public virtual void Initialize() => Init = true;

    public void Update()
    {
        if (!Init)
            Initialize();

        if (Room.IsObjectKilled(Id))
            return;

        InternalUpdate();
    }

    public virtual void InternalUpdate() { }

    public virtual void CheckForSpawner()
    {
        IsFromSpawner = true;

        var spawnerId = Id.Split("_");

        LinkedSpawner = Room.GetEntityFromId<BaseSpawnerControllerComp>(spawnerId[0]);

        if (LinkedSpawner == null)
            return;

        Position = new Vector3(
            LinkedSpawner.Position.X + LinkedSpawner.SpawningOffsetX,
            LinkedSpawner.Position.Y + LinkedSpawner.SpawningOffsetY,
            LinkedSpawner.Position.Z
        );

        ParentPlane = LinkedSpawner.ParentPlane;
    }

    public void GenerateHitbox(HitboxModel box)
    {
        var width = box.Width * EnemyController.Scale.X * (EnemyController.Scale.X < 0 ? -1 : 1);
        var height = box.Height * EnemyController.Scale.Y * (EnemyController.Scale.Y < 0 ? -1 : 1);

        var offsetX = box.XOffset * EnemyController.Scale.X - width / 2 * (EnemyController.Scale.X < 0 ? -1 : 1);
        var offsetY = box.YOffset * EnemyController.Scale.Y - height / 2 * (EnemyController.Scale.Y < 0 ? -1 : 1);

        var position = new Vector3Model { X = offsetX, Y = offsetY, Z = Position.z };

        Hitbox = new EnemyCollider(Id, position, width, height, ParentPlane, Room)
        {
            Position = new Vector3(Position.x, Position.y, Position.z)
        };

        Room.Colliders.Add(Id, Hitbox);
    }

    public virtual void Damage(int damage, Player origin)
    {
        if (Room.IsObjectKilled(Id))
            return;

        var trueDamage = damage - GameFlow.StatisticData.GetValue(ItemEffectType.Defence, WorldStatisticsGroup.Enemy, Level);

        if (trueDamage <= 0)
            trueDamage = 1;

        Health -= trueDamage;

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health, trueDamage, 0, 0, origin == null ? string.Empty : origin.CharacterName, false, true));

        if (Health <= 0)
        {
            if (OnDeathTargetId is not null and not "0")
                foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(OnDeathTargetId))
                    trigger.Trigger(true);

            //The XP Reward here is not accurate, but pretty close
            var xpAward = origin != null ? DeathXp - (origin.Character.Data.GlobalLevel - 1) * 5 : DeathXp;

            if (xpAward < 1)
                xpAward = 1;

            Room.SendSyncEvent(AISyncEventHelper.AIDie(Id, Room.Time, string.Empty, xpAward > 0 ? xpAward : 1, true, origin == null ? "0" : origin.GameObjectId, false));

            //Dynamic Loot Drop
            if (origin != null)
            {
                origin.AddReputation(xpAward > 0 ? xpAward : 1, ServerRConfig);

                if (EnemyModel.EnemyLootTable != null)
                {
                    var random = new System.Random();

                    foreach (var drop in EnemyModel.EnemyLootTable)
                    {
                        random.NextDouble();
                        if (Level <= drop.MaxLevel && Level >= drop.MinLevel)
                            origin.GrantDynamicLoot(Level, drop, ItemCatalog);
                    }
                }

                //Achievements
                origin.CheckObjective(ObjectiveEnum.Score, Id, EnemyController.PrefabName, 1, QuestCatalog);
                origin.CheckObjective(ObjectiveEnum.Scoremultiple, Id, EnemyController.PrefabName, 1, QuestCatalog);

                origin.CheckAchievement(AchConditionType.DefeatEnemy, string.Empty, InternalAchievement, Logger);
                origin.CheckAchievement(AchConditionType.DefeatEnemy, Enum.GetName(EnemyModel.EnemyCategory), InternalAchievement, Logger);
                origin.CheckAchievement(AchConditionType.DefeatEnemy, EnemyController.PrefabName, InternalAchievement, Logger);
                origin.CheckAchievement(AchConditionType.DefeatEnemyInLevel, origin.Room.LevelInfo.Name, InternalAchievement, Logger);
            }
            
            //For spawners
            if (IsFromSpawner)
            {
                LinkedSpawner.NotifyEnemyDefeat(Id);
                Room.Enemies.Remove(Id);
                Room.Colliders.Remove(Id);
            }

            Room.KillEntity(origin, Id);
        }
    }

    public virtual void SendAiData(Player player)
    {
    }

    public void Destroy(Player player, Room room, string id)
    {
        room.Enemies.Remove(id);
        room.Colliders.Remove(id);
    }
}
