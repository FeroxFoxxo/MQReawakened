using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.Characters.Controllers;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Spawners.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;

public abstract class BaseEnemy : IDestructible
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

    public BaseEnemy(EnemyData data)
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

        var position = new Vector3 { x = offsetX, y = offsetY, z = Position.z };

        Hitbox = new EnemyCollider(Id, position, new Vector2(width, height), ParentPlane, Room)
        {
            Position = new Vector3(
                Position.x + EnemyModel.Offset.X,
                Position.y + EnemyModel.Offset.Y,
                Position.z + EnemyModel.Offset.Z
            )
        };

        Room.AddCollider(Hitbox);
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

                origin.CheckAchievement(AchConditionType.DefeatEnemy, [Enum.GetName(EnemyModel.EnemyCategory), EnemyController.PrefabName], InternalAchievement, Logger);
                origin.CheckAchievement(AchConditionType.DefeatEnemyInLevel, [origin.Room.LevelInfo.Name], InternalAchievement, Logger);
            }

            //For spawners
            if (IsFromSpawner)
            {
                LinkedSpawner.NotifyEnemyDefeat(Id);
                Room.RemoveEnemy(Id);
            }

            Room.KillEntity(origin, Id);
        }
    }

    public virtual void SendAiData(Player player)
    {
    }

    public void Destroy(Player player, Room room, string id) => room.RemoveEnemy(id);

    public void Heal(int healPoints)
    {
        if (Room.IsObjectKilled(Id))
            return;

        Health += healPoints;

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health, healPoints, 0, 0, string.Empty, false, true));
    }
}
