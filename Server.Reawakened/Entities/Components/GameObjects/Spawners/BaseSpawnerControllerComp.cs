using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Spawners;

public class BaseSpawnerControllerComp : Component<BaseSpawnerController>
{
    private const int FinalizeSpawnDelaySeconds = 4;
    private const float NotScheduled = -1f;

    public int SpawnCycleCount => ComponentData.SpawnCycleCount;
    public int MaxSimultanousSpawned => ComponentData.MaxSimultanousSpawned;
    public float InitialSpawnDelay => ComponentData.InitialSpawnDelay;
    public float MinSpawnInterval => ComponentData.MinSpawnInterval;
    public string PrefabNameToSpawn1 => ComponentData.PrefabNameToSpawn1;
    public string PrefabNameToSpawn2 => ComponentData.PrefabNameToSpawn2;
    public string PrefabNameToSpawn3 => ComponentData.PrefabNameToSpawn3;
    public string PrefabNameToSpawn4 => ComponentData.PrefabNameToSpawn4;
    public string PrefabNameToSpawn5 => ComponentData.PrefabNameToSpawn5;
    public string TemplatePrefabNameToSpawn1 => ComponentData.TemplatePrefabNameToSpawn1;
    public string TemplatePrefabNameToSpawn2 => ComponentData.TemplatePrefabNameToSpawn2;
    public string TemplatePrefabNameToSpawn3 => ComponentData.TemplatePrefabNameToSpawn3;
    public string TemplatePrefabNameToSpawn4 => ComponentData.TemplatePrefabNameToSpawn4;
    public string TemplatePrefabNameToSpawn5 => ComponentData.TemplatePrefabNameToSpawn5;
    public float SpawningOffsetX => ComponentData.SpawningOffsetX;
    public float SpawningOffsetY => ComponentData.SpawningOffsetY;
    public int LevelOffset => ComponentData.LevelOffset;
    public bool CreateFXOnSpawn => ComponentData.CreateFXOnSpawn;
    public bool SpawnFromStart => ComponentData.SpawnFromStart;
    public bool SpawnOnDetection => ComponentData.SpawnOnDetection;
    public float DetectionRadius => ComponentData.DetectionRadius;
    public Vector3 PatrolDistance => ComponentData.PatrolDistance;
    public string OnDeathTargetID => ComponentData.OnDeathTargetID;

    public ILogger<BaseSpawnerControllerComp> Logger { get; set; }
    public InternalEnemyData EnemyInfoXml { get; set; }
    public IServiceProvider Services { get; set; }
    public TimerThread TimerThread { get; set; }

    public int Level;

    public Dictionary<string, BaseEnemy> LinkedEnemies;
    public Dictionary<string, EnemyModel> EnemyModels;

    private int _spawnedEntityCount;
    private float _nextSpawnRequestTime;
    private bool _spawnRequested;
    private bool _activated;
    private int _updatedSpawnCycle;

    private TriggerArenaComp _arenaComp;
    private TriggerProtectionArenaComp _protectArenaComp;
    private BreakableEventControllerComp _breakableComp;

    private int _healthMod, _scaleMod, _resMod;
    private int _stars, _currentHealth, _maxHealth;
    private List<(string prefab, string template)> _spawnOptions;

    private bool _pendingDestroy;

    public bool HasLinkedArena => _arenaComp != null;
    public int Difficulty => _breakableComp != null ? _breakableComp.Damageable.DifficultyLevel : Room.LevelInfo.Difficulty;
    public int Stars => _breakableComp != null ? _breakableComp.Damageable.Stars : _stars;
    public int CurrentHealth => _breakableComp != null ? _breakableComp.Damageable.CurrentHealth : _currentHealth;
    public int MaxHealth => _breakableComp != null ? _breakableComp.Damageable.MaxHealth : _maxHealth;

    public override void InitializeComponent()
    {
        _healthMod = 1;
        _scaleMod = 1;
        _resMod = 1;

        _stars = 1;
        _maxHealth = 5;
        _currentHealth = _maxHealth;

        _breakableComp = Room.GetEntityFromId<BreakableEventControllerComp>(Id);
        LinkedEnemies = [];
        EnemyModels = [];

        ResetSpawnCycle();

        Position.SetPositionViaPlane(ParentPlane, PrefabName, Logger);

        AddEnemyModel(PrefabNameToSpawn1);
        AddEnemyModel(PrefabNameToSpawn2);
        AddEnemyModel(PrefabNameToSpawn3);
        AddEnemyModel(PrefabNameToSpawn4);
        AddEnemyModel(PrefabNameToSpawn5);

        _spawnOptions = [.. new List<(string prefab, string template)>
        {
            (PrefabNameToSpawn1, TemplatePrefabNameToSpawn1),
            (PrefabNameToSpawn2, TemplatePrefabNameToSpawn2),
            (PrefabNameToSpawn3, TemplatePrefabNameToSpawn3),
            (PrefabNameToSpawn4, TemplatePrefabNameToSpawn4),
            (PrefabNameToSpawn5, TemplatePrefabNameToSpawn5)
        }
        .Where(p => !string.IsNullOrWhiteSpace(p.prefab) && !string.IsNullOrWhiteSpace(p.template))];
    }

    public override void DelayedComponentInitialization()
    {
        Level = Math.Max(1, Room.LevelInfo.Difficulty + LevelOffset);
        SetActive(_arenaComp is null);
    }

    public void AddEnemyModel(string prefabName)
    {
        if (string.IsNullOrEmpty(prefabName))
            return;

        if (EnemyInfoXml.EnemyInfoCatalog.TryGetValue(prefabName, out var enemyModel))
            EnemyModels.TryAdd(prefabName, enemyModel);
        else
            Logger.LogError("Unknown enemy to add to spawner: '{EnemyPrefab}'", prefabName);
    }

    public void SetActive(bool result)
    {
        if (_breakableComp != null)
            _breakableComp.CanBreak = result;

        _activated = result;
    }

    public override void Update()
    {
        if (!_activated)
            return;

        var position = new Vector3(Position.X, Position.Y, Position.Z);

        if (Room == null)
            return;

        var triggerSpawn = false;
        var triggerFinalize = false;

        if (Room.IsPlayerNearby(position, DetectionRadius) &&
            LinkedEnemies.Count < MaxSimultanousSpawned &&
            _nextSpawnRequestTime <= 0)
        {
            triggerSpawn = true;
        }

        if (_spawnRequested && _nextSpawnRequestTime <= Room.Time)
            triggerFinalize = true;

        if (triggerSpawn)
            Spawn();
        
        if (triggerFinalize)
            SpawnEventCalled(FinalizeSpawnDelaySeconds);
    }

    public void Spawn()
    {
        _nextSpawnRequestTime = _nextSpawnRequestTime == NotScheduled ? Room.Time + InitialSpawnDelay : Room.Time + MinSpawnInterval;

        if (CanSpawnMoreThisCycle() && LinkedEnemies.Count < MaxSimultanousSpawned)
            _spawnRequested = true;
    }

    private void ActivateArenaSpawn(Action setArena)
    {
        setArena();
        SetActive(true);
        Spawn();
    }

    public void Spawn(TriggerArenaComp arena) => ActivateArenaSpawn(() => SetArena(arena));
    public void Spawn(TriggerProtectionArenaComp arena) => ActivateArenaSpawn(() => SetArena(arena));

    public void Despawn()
    {
        foreach (var enemy in LinkedEnemies.Values)
            enemy.DespawnEnemy();
    }

    public void SetArena(TriggerArenaComp arena) => _arenaComp = arena;
    public void SetArena(TriggerProtectionArenaComp arena) => _protectArenaComp = arena;
    
    public void RemoveFromArena()
    {
        if (_arenaComp == null)
            return;

        var toRemove = _arenaComp.ArenaEntities.Where(e => e == Id).ToList();

        foreach (var e in toRemove)
            _arenaComp.ArenaEntities.Remove(e);
    }

    public void Revive()
    {
        _breakableComp?.Respawn();

        SetActive(false);

        ResetSpawnCycle();

        Room.ToggleCollider(Id, true);
    }

    private void SpawnEventCalled(int delay)
    {
        string selectedPrefab;
        string templateId;

        _spawnRequested = false;

        if (_spawnOptions == null || _spawnOptions.Count == 0)
        {
            Logger.LogError("No valid prefab/template pairs configured for spawner with id: {Id}. Returning...", Id);
            return;
        }

        _spawnedEntityCount++;

        var index = (_spawnedEntityCount - 1) % _spawnOptions.Count;
        (selectedPrefab, templateId) = _spawnOptions[index];

        if (!EnemyModels.TryGetValue(selectedPrefab, out var enemyToSpawn) || enemyToSpawn is null)
        {
            Logger.LogError("Enemy model for prefab '{Prefab}' not found for spawner id: {Id}. Returning...", selectedPrefab, Id);
            return;
        }

        var states = new Dictionary<StateType, BaseState>();
        var behaviorsMap = new Dictionary<StateType, AIBaseBehavior>();

        var genericComp = Room.GetEntityFromId<AIStatsGenericComp>(templateId);
        var globalComp = Room.GetEntityFromId<AIStatsGlobalComp>(templateId);

        Logger.LogInformation("Spawner '{Id}' spawning enemy #{Num} prefab '{Prefab}' template '{Template}'", Id, _spawnedEntityCount, selectedPrefab, templateId);

        // Make sure to modify patrol data before init to account for patrol range
        if (genericComp != null)
        {
            genericComp.PatrolX = PatrolDistance.x;
            genericComp.PatrolY = PatrolDistance.y;
        }

        // Init event must be called before AIDo or else the enemy freezes
        Room.SendSyncEvent(
            AISyncEventHelper.AIInit(
                Id, Room,
                Position.X, Position.Y, Position.Z, Position.X, Position.Y,
                genericComp?.Patrol_InitialProgressRatio ?? 0f, CurrentHealth, MaxHealth,
                _healthMod, _scaleMod, _resMod, Stars, Level, globalComp?.GetGlobalProperties(), states, behaviorsMap
            )
        );

        Room.SendSyncEvent(
            AISyncEventHelper.AIDo(
                Id, Room,
                0, 0, 1.0f,
                Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY, genericComp?.Patrol_ForceDirectionX ?? 0,
                false, AISyncEventHelper.IndexOf(StateType.Unknown, enemyToSpawn.BehaviorData ?? []), string.Empty
            )
        );

        Room.SendSyncEvent(new Spawn_SyncEvent(Id, Room.Time, _spawnedEntityCount));

        TimerThread.RunDelayed(DelayedSpawnData, new DelayedEnemySpawn() { Spawner = this, TemplateId = templateId, PrefabName = selectedPrefab, SpawnIndex = _spawnedEntityCount }, TimeSpan.FromSeconds(delay));
    }

    private void ResetSpawnCycle()
    {
        _nextSpawnRequestTime = NotScheduled;
        _spawnRequested = false;
        _spawnedEntityCount = 0;
        _updatedSpawnCycle = SpawnCycleCount;
        LinkedEnemies.Clear();
    }

    public class DelayedEnemySpawn() : ITimerData
    {
        public BaseSpawnerControllerComp Spawner;
        public string TemplateId;
        public string PrefabName;
        public int SpawnIndex;

        public bool IsValid() => Spawner != null && Spawner.IsValid();
    }

    private static void DelayedSpawnData(ITimerData data)
    {
        if (data is not DelayedEnemySpawn spawn)
            return;

        var spawner = spawn.Spawner;
        var templateId = spawn.TemplateId;
        var prefabName = spawn.PrefabName;

        spawner._nextSpawnRequestTime = 0;

        var room = spawner.Room;
        var spawnedEntityId = $"{spawner.Id}_{spawn.SpawnIndex}";

        var templateGo = room.Planes.Values
            .SelectMany(p => p.GameObjects)
            .Where(kvp => kvp.Key == templateId)
            .SelectMany(kvp => kvp.Value)
            .FirstOrDefault();

        var newObjectInfo = new ObjectInfoModel
        {
            ObjectId = spawnedEntityId,
            PrefabName = prefabName,
            ParentPlane = spawner.ParentPlane,
            Position = new Vector3Model(spawner.Position.X + spawner.SpawningOffsetX, spawner.Position.Y + spawner.SpawningOffsetY, spawner.Position.Z),
            Rotation = new Vector3Model(templateGo.ObjectInfo.Rotation.X, templateGo.ObjectInfo.Rotation.Y, templateGo.ObjectInfo.Rotation.Z),
            Scale = new Vector3Model(templateGo.ObjectInfo.Scale.X, templateGo.ObjectInfo.Scale.Y, templateGo.ObjectInfo.Scale.Z),
            Rectangle = templateGo.ObjectInfo.Rectangle,
            Components = templateGo.ObjectInfo.Components.ToDictionary(k => k.Key, v => new ComponentModel { ComponentAttributes = new Dictionary<string, string>(v.Value.ComponentAttributes) })
        };

        var builder = spawner.Services.GetRequiredService<EntityComponentBuilder>();
        var builtComponents = builder.Build(new GameObjectModel { ObjectInfo = newObjectInfo }, room, out var _);

        if (builtComponents.Count == 0)
        {
            spawner.Logger.LogError("Failed to build components for spawned enemy {SpawnedId} using template {TemplateId}", spawnedEntityId, templateId);
            return;
        }

        spawner.Room.AddEntity(spawnedEntityId, builtComponents);

        foreach (var component in builtComponents)
        {
            try
            {
                component.InitializeComponent();
            }
            catch (Exception e)
            {
                spawner.Logger.LogError(e, "Error initializing spawned component {Component} for {SpawnedId}", component.GetType().Name, spawnedEntityId);
            }
        }

        foreach (var component in builtComponents)
        {
            try
            {
                component.DelayedComponentInitialization();
            }
            catch (Exception e)
            {
                spawner.Logger.LogError(e, "Error delayed initializing spawned component {Component} for {SpawnedId}", component.GetType().Name, spawnedEntityId);
            }
        }

        room.GetEntityFromId<AIStatsGenericComp>(spawnedEntityId)?.SetPatrolRange(spawner.PatrolDistance);

        var newEnemy = room.GetEnemyFromId(spawnedEntityId).Enemy;

        if (newEnemy is not null)
        {
            newEnemy.LinkSpawner(spawner);

            if (!spawner.LinkedEnemies.TryAdd(spawnedEntityId, newEnemy))
                spawner.Logger?.LogWarning("Attempted to add duplicate LinkedEnemies key {SpawnedId} to spawner {SpawnerId}", spawnedEntityId, spawner.Id);
        }

        if (spawner._arenaComp != null)
        {
            if (!spawner._arenaComp.ArenaEntities.Contains(spawnedEntityId))
            {
                spawner._arenaComp.ArenaEntities.Add(spawnedEntityId);
                spawner.Logger?.LogDebug("Added spawned enemy {SpawnedId} to arena (arena id: {ArenaId})", spawnedEntityId, spawner._arenaComp.Id);
            }
            else
            {
                spawner.Logger?.LogWarning("Duplicate spawned enemy id {SpawnedId} attempted to be added to arena {ArenaId}", spawnedEntityId, spawner._arenaComp.Id);
            }
        }
        else
        {
            spawner.Logger?.LogDebug("Spawner {SpawnerId} spawned {SpawnedId} but no arena linked", spawner.Id, spawnedEntityId);
        }
    }

    public void NotifyEnemyDefeat(string id)
    {
        bool waveCleared;
        bool moreThisCycle;

        LinkedEnemies.Remove(id);

        if (_arenaComp != null)
        {
            var removed = _arenaComp.ArenaEntities.Remove(id);
            if (removed) Logger?.LogDebug("Removed spawned enemy {SpawnedId} from arena (arena id: {ArenaId})", id, _arenaComp.Id);
        }

        waveCleared = LinkedEnemies.Count == 0;
        moreThisCycle = CanSpawnMoreThisCycle();

        if (waveCleared)
        {
            if (moreThisCycle)
            {
                _spawnRequested = true;
                _nextSpawnRequestTime = MinSpawnInterval <= 0 ? Room.Time : Room.Time + MinSpawnInterval;
            }
            else
            {
                if (SpawnCycleCount > 0)
                    _updatedSpawnCycle += SpawnCycleCount;
                
                _nextSpawnRequestTime = 0;
                
                if (_pendingDestroy)
                {
                    Room.RemoveEnemy(Id);
                    _pendingDestroy = false;
                }
            }
        }

        if (SpawnCycleCount == 1)
            PingDeathTargets();

        _protectArenaComp?.AddDefeat();
    }

    private bool CanSpawnMoreThisCycle() => SpawnCycleCount <= 0 || _spawnedEntityCount < _updatedSpawnCycle;

    public void Destroy()
    {
        if (LinkedEnemies.Count > 0)
        {
            _pendingDestroy = true;
            return;
        }
        
        Room.RemoveEnemy(Id);
    }

    public void PingDeathTargets()
    {
        foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(OnDeathTargetID))
            trigger.TriggerStateChange(TriggerType.Activate, true, Id);
    }
}
