using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Hazards;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Components.GameObjects.Spawners;

public class BaseSpawnerControllerComp : Component<BaseSpawnerController>
{
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
    public UnityEngine.Vector3 PatrolDistance => ComponentData.PatrolDistance;
    public string OnDeathTargetID => ComponentData.OnDeathTargetID;

    public ILogger<BaseSpawnerControllerComp> Logger { get; set; }
    public InternalEnemyData EnemyInfoXml { get; set; }
    public IServiceProvider Services { get; set; }
    public TimerThread TimerThread { get; set; }

    public int Level;

    public Dictionary<string, BaseEnemy> LinkedEnemies;
    public Dictionary<string, EnemyModel> EnemyModels;
    public Dictionary<string, SpawnedEnemyData> TemplateEnemyModels;

    private int _spawnedEntityCount;
    private float _nextSpawnRequestTime;
    private bool _spawnRequested;
    private bool _activated;
    private float _activeDetectionRadius;
    private int _updatedSpawnCycle;

    private TriggerArenaComp _arenaComp;
    private TriggerProtectionArenaComp _protectArenaComp;
    private BreakableEventControllerComp _breakableComp;

    private int _healthMod, _scaleMod, _resMod;
    private int _stars, _currentHealth, _maxHealth;

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

        _spawnedEntityCount = 0;
        _nextSpawnRequestTime = -1;
        _spawnRequested = false;
        _activeDetectionRadius = DetectionRadius;
        _updatedSpawnCycle = SpawnCycleCount;

        LinkedEnemies = [];
        EnemyModels = [];
        TemplateEnemyModels = [];

        Position.SetPositionViaPlane(ParentPlane, PrefabName, Logger);

        AddEnemyModel(PrefabNameToSpawn1);
        AddEnemyModel(PrefabNameToSpawn2);
        AddEnemyModel(PrefabNameToSpawn3);
        AddEnemyModel(PrefabNameToSpawn4);
        AddEnemyModel(PrefabNameToSpawn5);

        AddTemplateModel(TemplatePrefabNameToSpawn1);
        AddTemplateModel(TemplatePrefabNameToSpawn2);
        AddTemplateModel(TemplatePrefabNameToSpawn3);
        AddTemplateModel(TemplatePrefabNameToSpawn4);
        AddTemplateModel(TemplatePrefabNameToSpawn5);

    }

    public override void DelayedComponentInitialization()
    {
        Level = Room.LevelInfo.Difficulty + LevelOffset;
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

    public void AddTemplateModel(string templateId)
    {
        if (string.IsNullOrEmpty(templateId))
            return;

        var global = Room.GetEntityFromId<AIStatsGlobalComp>(templateId);
        var generic = Room.GetEntityFromId<AIStatsGenericComp>(templateId);
        var status = Room.GetEntityFromId<InterObjStatusComp>(templateId);
        var hazard = Room.GetEntityFromId<HazardControllerComp>(templateId);

        var enemyController = Room.GetEnemyFromId(templateId);

        if (global is null || generic is null || status is null || enemyController is null || hazard is null)
        {
            Logger.LogError("Unknown enemy template to add to spawner: '{EnemyPrefab}'", templateId);
            return;
        }

        var spawnerData = new SpawnedEnemyData(global, generic, status, enemyController, hazard, global.GetGlobalProperties());

        TemplateEnemyModels.TryAdd(templateId, spawnerData);
    }

    public override void Update()
    {
        if (_activated)
        {
            var position = new UnityEngine.Vector3(Position.X, Position.Y, Position.Z);

            if (Room == null)
                return;

            if (Room.IsPlayerNearby(position, _activeDetectionRadius) && LinkedEnemies.Count < 1 && _nextSpawnRequestTime <= 0)
                Spawn();

            if (_spawnRequested && _nextSpawnRequestTime <= Room.Time)
                //NOT A MAGIC NUMBER. This is a constant defined in BaseSpawnerController
                SpawnEventCalled(4);
        }
    }

    public void Spawn()
    {
        _nextSpawnRequestTime = _nextSpawnRequestTime == -1 ? Room.Time + InitialSpawnDelay : Room.Time + MinSpawnInterval;

        if (_spawnedEntityCount < _updatedSpawnCycle)
            _spawnRequested = true;
    }

    public void Spawn(TriggerArenaComp arena)
    {
        SetArena(arena);

        _activeDetectionRadius = 100;
        SetActive(true);

        Spawn();
    }

    public void Spawn(TriggerProtectionArenaComp arena)
    {
        SetArena(arena);

        _activeDetectionRadius = 100;
        SetActive(true);

        Spawn();
    }

    public void Despawn()
    {
        foreach (var enemy in LinkedEnemies)
            enemy.Value.DespawnEnemy();
    }

    public void SetArena(TriggerArenaComp arena) => _arenaComp = arena;
    public void SetArena(TriggerProtectionArenaComp arena) => _protectArenaComp = arena;
    public void RemoveFromArena() => _arenaComp.ArenaEntities.Remove(Id);

    public void Revive()
    {
        _breakableComp?.Respawn();
        SetActive(false);

        _activeDetectionRadius = DetectionRadius;
        _nextSpawnRequestTime = -1;
        _spawnRequested = false;
        _updatedSpawnCycle = _spawnedEntityCount + SpawnCycleCount;
        LinkedEnemies.Clear();

        Room.ToggleCollider(Id, true);
    }

    private void SpawnEventCalled(int delay)
    {
        _spawnedEntityCount++;
        _spawnRequested = false;

        // TODO: Change to spawn correct enemy; not just first.
        var enemyToSpawn = EnemyModels.Values.FirstOrDefault();
        var templateToSpawnAt = TemplateEnemyModels.Values.FirstOrDefault();

        if (enemyToSpawn is null || templateToSpawnAt is null)
        {
            Logger.LogError("Either enemy to spawn and template is null for spawner with id: {Id}. Returning...", Id);
            return;
        }

        var behaviors = new Dictionary<StateType, BaseState>
        {
            { StateType.Idle, new IdleState([]) }
        };

        Room.SendSyncEvent(
            AISyncEventHelper.AIInit(
                Id, Room,
                Position.X, Position.Y, Position.Z, Position.X, Position.Y,
                templateToSpawnAt.Generic.Patrol_InitialProgressRatio, CurrentHealth, MaxHealth,
                _healthMod, _scaleMod, _resMod, Stars, Level, templateToSpawnAt.GlobalProperties, behaviors
            )
        );

        Room.SendSyncEvent(
            AISyncEventHelper.AIDo(
                Id, Room,
                0, 0, 1.0f,
                Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY, templateToSpawnAt.Generic.Patrol_ForceDirectionX,
                false, AISyncEventHelper.IndexOf(StateType.Unknown, enemyToSpawn.BehaviorData ?? []), string.Empty
            )
        );

        _arenaComp?.ArenaEntities.Add($"{Id}_{_spawnedEntityCount}");

        Room.SendSyncEvent(new Spawn_SyncEvent(Id, Room.Time, _spawnedEntityCount));

        TimerThread.RunDelayed(DelayedSpawnData, new DelayedEnemySpawn() { Data = templateToSpawnAt, Spawner = this }, TimeSpan.FromSeconds(delay));
    }

    public class DelayedEnemySpawn() : ITimerData
    {
        public SpawnedEnemyData Data;
        public BaseSpawnerControllerComp Spawner;

        public bool IsValid() => Data != null && Spawner != null && Spawner.IsValid();
    }

    private static void DelayedSpawnData(ITimerData data)
    {
        if (data is not DelayedEnemySpawn spawn)
            return;

        var spawner = spawn.Spawner;
        var enemy = spawn.Data;

        spawner._nextSpawnRequestTime = 0;

        BaseComponent enemyComponent = null;

        if (enemy.EnemyController is EnemyControllerComp controllerComp)
            enemyComponent = controllerComp;
        else if (enemy.EnemyController is ArmoredEnemyControllerComp armoredEnemyController)
            enemyComponent = armoredEnemyController;
        else
            spawner.Logger.LogError("Unknown enemy component of type {Type}", enemyComponent.GetType().Name);

        //Set all component data
        var newEntity = new List<BaseComponent>
        {
            enemy.Global,
            enemy.Generic,
            enemy.Status,
            enemyComponent,
            enemy.Hazard
        };

        var _spawnedEntityId = $"{spawner.Id}_{spawner._spawnedEntityCount}";

        spawner.Room.AddEntity(_spawnedEntityId, newEntity);

        foreach (var component in newEntity)
        {
            component.InitializeComponent();
            spawner.Room.RemoveKilledEnemy(component.Id);
        }

        //Fix some things before setting the enemy
        enemy.Hazard.SetId(_spawnedEntityId);
        enemy.Generic.SetPatrolRange(spawner.PatrolDistance);

        var newEnemy = enemy.EnemyController.CreateEnemy(_spawnedEntityId, spawner.PrefabNameToSpawn1);

        if (newEnemy is not null)
            spawner.LinkedEnemies.Add(_spawnedEntityId, newEnemy);
    }

    public void NotifyEnemyDefeat(string id)
    {
        LinkedEnemies.Remove(id);
        _protectArenaComp?.AddDefeat();
    }

    public void Destroy() => Room.RemoveEnemy(Id);
}
