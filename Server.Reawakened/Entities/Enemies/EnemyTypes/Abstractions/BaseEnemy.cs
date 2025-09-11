using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Components.PrefabInfos;
using Server.Reawakened.Entities.Components.PrefabInfos.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;

public abstract class BaseEnemy : IDestructible
{
    public readonly ILogger<BaseEnemy> Logger;
    public readonly InternalAchievement InternalAchievement;
    public readonly QuestCatalog QuestCatalog;
    public readonly ItemCatalog ItemCatalog;
    public readonly ServerRConfig ServerRConfig;

    public readonly Room Room;

    public bool Init;

    public string Id;
    public EnemyCollider Hitbox;
    public string ParentPlane;
    public bool IsFromSpawner;

    public readonly string PrefabName;

    public int Health;
    public int MaxHealth;
    public int Level;
    public int DeathXp;
    public string OnDeathTargetId;

    public float HealthModifier;
    public float ScaleModifier;
    public float ResistanceModifier;
    public int Stars;

    public BaseSpawnerControllerComp LinkedSpawner;

    public readonly IEnemyController EnemyController;
    public readonly EnemyModel EnemyModel;
    public InterObjStatusComp Status;

    public Vector3Model Position => EnemyController.Position;

    public IServiceProvider Services;

    public BaseEnemy(EnemyData data)
    {
        Room = data.Room;
        Id = data.EntityId;
        PrefabName = data.PrefabName;
        EnemyController = data.EnemyController;
        EnemyModel = data.EnemyModel;
        Services = data.Services;

        IsFromSpawner = false;

        Logger = Services.GetRequiredService<ILogger<BaseEnemy>>();
        InternalAchievement = Services.GetRequiredService<InternalAchievement>();
        QuestCatalog = Services.GetRequiredService<QuestCatalog>();
        ItemCatalog = Services.GetRequiredService<ItemCatalog>();
        ServerRConfig = Services.GetRequiredService<ServerRConfig>();

        Logger.LogDebug("Creating enemy {PrefabName} with ID {Id}", PrefabName, Id);

        ParentPlane = EnemyController.ParentPlane;

        var zPosition = EnemyController.Position.Z;

        switch (ParentPlane)
        {
            case "TemplatePlane":
                break;
            case "Plane1":
                zPosition = 20;
                break;
            case "Plane0":
                zPosition = 0;
                break;
            default:
                Logger.LogError("Unknown plane: '{Plane}' for enemy {Name}", ParentPlane, PrefabName);
                break;
        }

        EnemyController.Position.SetPosition(
            EnemyController.Position.X,
            EnemyController.Position.Y,
            zPosition
        );

        Status = Room.GetEntityFromId<InterObjStatusComp>(Id);

        OnDeathTargetId = EnemyController.OnDeathTargetID;

        Level = Room.LevelInfo.Difficulty + EnemyController.EnemyLevelOffset;

        DeathXp = GameFlow.StatisticData.GetValue(ItemEffectType.IncreaseExperience, WorldStatisticsGroup.Enemy, Level);
        MaxHealth = GameFlow.StatisticData.GetValue(ItemEffectType.IncreaseHitPoints, WorldStatisticsGroup.Enemy, Level);

        Health = MaxHealth;
        Logger.LogTrace("Spawn health for {PrefabName} (ID: {Id}) => {Health}/{Max}", PrefabName, Id, Health, MaxHealth);

        HealthModifier = 1;
        ScaleModifier = 1;
        ResistanceModifier = 1;
    }

    public virtual void Initialize() {
        Logger.LogDebug("Initializing enemy {PrefabName} (ID: {Id})", PrefabName, Id);

        GenerateHitbox();
        
        Init = true;
    }

    public void Update()
    {
        if (!Init)
            Initialize();

        if (Room.IsObjectKilled(Id))
            return;

        InternalUpdate();
    }

    public virtual void InternalUpdate() { }

    protected virtual bool ApplyFlipYOffset() => EnemyController?.Scale != null && EnemyController.Scale.Y < 0;

    public void LinkSpawner(BaseSpawnerControllerComp linkedSpawner)
    {
        LinkedSpawner = linkedSpawner;
        IsFromSpawner = true;

        Position.SetPosition(
            LinkedSpawner.Position.X + LinkedSpawner.SpawningOffsetX,
            LinkedSpawner.Position.Y + LinkedSpawner.SpawningOffsetY,
            LinkedSpawner.Position.Z
        );

        ParentPlane = LinkedSpawner.ParentPlane;
    }

    public void GenerateHitbox()
    {
        var serverObjectSize = Room.GetEntityFromId<ServerObjectSizeInfoComp>(Id);
        var objectSize = Room.GetEntityFromId<ObjectSizeInfoComp>(Id);

        Logger.LogDebug("Generating hitbox for enemy {PrefabName} (ID: {Id})", PrefabName, Id);

        IObjectSizeInfo box = serverObjectSize != null ? serverObjectSize : objectSize;

        if (box == null || EnemyController.Scale == null)
        {
            Logger.LogError("Box or Scale is null for enemy {PrefabName} with ID {Id}. Cannot generate hitbox.", PrefabName, Id);
            Logger.LogError("Box: {Box}, Scale: {Scale}", box != null ? "Present" : "Null", EnemyController.Scale != null ? "Present" : "Null");
            
            var allComponents = Room.GetEntitiesFromId<BaseComponent>(Id);
            
            Logger.LogError("Available components for enemy {Id}: {Components}", Id, 
                string.Join(", ", allComponents.Select(c => c.GetType().Name)));
            
            return;
        }

        var size = box.GetSize();
        var offset = box.GetOffset();

        var width = size.x * Math.Abs(EnemyController.Scale.X);
        var height = size.y * Math.Abs(EnemyController.Scale.Y);

        var bottomX = offset.x * EnemyController.Scale.X;
        var bottomY = offset.y * EnemyController.Scale.Y;

        var rect = new RectModel(bottomX - width, bottomY, width, height);

        Logger.LogTrace("Created enemy hitbox at {Position} of size {Size}", Position, rect);

        Hitbox = new EnemyCollider(this, rect);
    }

    public virtual void Damage(Player player, int damage)
    {
        if (Room.IsObjectKilled(Id))
            return;

        var resistance = GameFlow.StatisticData.GetValue(ItemEffectType.Defence, WorldStatisticsGroup.Enemy, Level);
        var resistedDamage = damage - resistance;

        if (resistedDamage <= 0)
            resistedDamage = 1;

        Health -= resistedDamage;

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health, damage, resistance, resistedDamage, player == null ? string.Empty : player.CharacterName, false, true));

        NotifyDamaged(player);
    }

    public virtual void PetDamage(Player player)
    {
        if (Room.IsObjectKilled(Id) || player == null)
            return;

        if (!player.Character.Pets.TryGetValue(player.GetEquippedPetId(ServerRConfig), out var pet))
        {
            Logger.LogError("Could not find pet that damaged {PrefabName}! Returning...", PrefabName);
            return;
        }
;
        var petDamage = (int)Math.Ceiling(MaxHealth * pet.AbilityParams.ItemEffectStatRatio);

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health -= petDamage, petDamage, 1, 1, player.CharacterName, false, true));

        NotifyDamaged(player);
    }

    public void NotifyDamaged(Player player)
    {
        var isDead = Health <= 0;

        DamagedEnemy(isDead);

        if (isDead)
            KillEnemy(player);
    }

    public void KillEnemy(Player player)
    {
        if (OnDeathTargetId is not null and not "0")
            foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(OnDeathTargetId))
                trigger.TriggerStateChange(TriggerType.Activate, true, Id);

        if (IsFromSpawner)
            LinkedSpawner.NotifyEnemyDefeat(Id);

        Destroy(Room, Id);
        Room.KillEntity(Id);

        SendRewards(player);
    }

    public void DamagedEnemy(bool isDead)
    {
        foreach (var damageComponent in Room.GetEntitiesFromId<IAIDamageEnemy>(Id))
            damageComponent.EnemyDamaged(isDead);
    }

    public void DespawnEnemy()
    {
        LinkedSpawner.NotifyEnemyDefeat(Id);

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, 0, 1, 0, 0, string.Empty, true, false));
        Destroy(Room, Id);
        Room.KillEntity(Id);
    }

    private void SendRewards(Player player)
    {
        var xpAward = player != null ? DeathXp - (player.Character.GlobalLevel - 1) * 5 : DeathXp;

        if (xpAward < 1)
            xpAward = 1;

        if (player != null)
        {
            player.AddReputation(xpAward > 0 ? xpAward : 1, ServerRConfig);
            
            if (EnemyModel.EnemyLootTable != null)
            {
                var random = new System.Random();

                foreach (var drop in EnemyModel.EnemyLootTable)
                {
                    random.NextDouble();
                    if (Level <= drop.MaxLevel && Level >= drop.MinLevel)
                        player.GrantDynamicLoot(Level, drop, ItemCatalog);
                }
            }

            foreach (var roomPlayer in Room.GetPlayers())
            {
                roomPlayer.CheckObjective(ObjectiveEnum.Score, Id, EnemyController.PrefabName, 1, QuestCatalog);
                roomPlayer.CheckObjective(ObjectiveEnum.Scoremultiple, Id, EnemyController.PrefabName, 1, QuestCatalog);
            }
        }
        
        Room.SendSyncEvent(AISyncEventHelper.AIDie(Id, Room.Time, string.Empty, xpAward > 0 ? xpAward : 1, true, player == null ? "0" : player.GameObjectId, false));

        if (player != null)
        {
            player.CheckAchievement(AchConditionType.DefeatEnemy, [PrefabName], InternalAchievement, Logger);
            player.CheckAchievement(AchConditionType.DefeatEnemy, [Enum.GetName(EnemyModel.EnemyCategory)], InternalAchievement, Logger);
            player.CheckAchievement(AchConditionType.DefeatEnemy, [EnemyController.PrefabName], InternalAchievement, Logger);
            player.CheckAchievement(AchConditionType.DefeatEnemyInLevel, [player.Room.LevelInfo.Name], InternalAchievement, Logger);
        }
    }

    public abstract void SendAiData(Player player);

    public void Destroy(Room room, string id) => room.RemoveEnemy(id);

    public void Heal(int healPoints)
    {
        if (Room.IsObjectKilled(Id))
            return;

        Health += healPoints;

        Room.SendSyncEvent(new AiHealth_SyncEvent(Id.ToString(), Room.Time, Health, healPoints, 0, 0, string.Empty, false, true));
    }

    public void FireProjectile(Vector3Model position, Vector2 speed, bool isGrenade) =>
        Room.AddRangedProjectile(Id, position, speed, 3, GetDamage(), EnemyController.EnemyEffectType, isGrenade);

    public int GetDamage() =>
        GameFlow.StatisticData.GetValue(
            ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, Level
        );
    
    public virtual void OnCollideWithPlayer(Player player) {}
}
