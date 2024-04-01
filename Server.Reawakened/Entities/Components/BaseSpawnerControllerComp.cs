using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Models;
using UnityEngine;

namespace Server.Reawakened.Entities.Components;

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
    public Vector3 PatrolDistance => ComponentData.PatrolDistance;
    public string OnDeathTargetID => ComponentData.OnDeathTargetID;
    public ILogger<BaseSpawnerControllerComp> Logger { get; set; }
    public InternalDefaultEnemies EnemyInfoXml { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public IServiceProvider Services { get; set; }
    public TimerThread TimerThread { get; set; }

    public int Health;
    public int Level;
    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public InterObjStatusComp Status;
    public EnemyControllerComp EnemyController;
    public HazardControllerComp Hazard;
    public GlobalProperties GlobalProperties;
    public BehaviorModel BehaviorList;

    public Dictionary<int, BehaviorEnemy> LinkedEnemies;
    private int _spawnedEntityCount;
    private float _nextSpawnRequestTime;
    private bool _spawnRequested;
    private bool _activated;
    private float _activeDetectionRadius;
    private TriggerArenaComp _arena;
    private string _spawnedEntityId;

    public override void InitializeComponent()
    {
        //Everything here is temporary until I add that world statistics xml thingy
        Level = Room.LevelInfo.Difficulty + LevelOffset;
        Health = 9999;
        _spawnedEntityCount = 0;
        _nextSpawnRequestTime = 0;
        _spawnRequested = false;
        _activated = false;
        _activeDetectionRadius = DetectionRadius;

        Global = Room.GetEntityFromId<AIStatsGlobalComp>(TemplatePrefabNameToSpawn1);
        Generic = Room.GetEntityFromId<AIStatsGenericComp>(TemplatePrefabNameToSpawn1);
        Status = Room.GetEntityFromId<InterObjStatusComp>(TemplatePrefabNameToSpawn1);
        EnemyController = Room.GetEntityFromId<EnemyControllerComp>(TemplatePrefabNameToSpawn1);
        Hazard = Room.GetEntityFromId<HazardControllerComp>(TemplatePrefabNameToSpawn1);

        //This is just a dummy, it gets assigned properly later in Enemy
        GlobalProperties = new GlobalProperties(true, 0, 0, 0, 0, 0, 0, 0, 0, 0, "Generic", string.Empty, false, false, 0);

        BehaviorList = EnemyInfoXml.GetBehaviorsByName(PrefabNameToSpawn1);
        LinkedEnemies = [];

        if (ComponentData.SpawnOnDetection)
            _activated = true;
    }

    public override void Update()
    {
        if (_activated && IsPlayerNearby(_activeDetectionRadius) && LinkedEnemies.Count < 1 && _nextSpawnRequestTime == 0)
            Spawn();
        if (_activated && _spawnRequested && _nextSpawnRequestTime <= Room.Time)
            //NOT A MAGIC NUMBER. This is a constant defined in BaseSpawnerController
            SpawnEventCalled(4);
    }

    public void Spawn()
    {
        _nextSpawnRequestTime = _spawnedEntityCount == 0 ? Room.Time + InitialSpawnDelay : Room.Time + MinSpawnInterval;

        if (_spawnedEntityCount < SpawnCycleCount)
            _spawnRequested = true;
    }

    public void Spawn(TriggerArenaComp arena)
    {
        _activated = true;
        _activeDetectionRadius = 20;
        Spawn();

        _arena = arena;
    }

    private void SpawnEventCalled(int delay)
    {
        _spawnedEntityCount++;
        _spawnRequested = false;

        //Spawn the enemy and set it in the room enemy list
        Room.SendSyncEvent(new AIInit_SyncEvent(Id, Room.Time, Position.X, Position.Y, Position.Z,
            Position.X, Position.Y, Generic.Patrol_InitialProgressRatio,
        Health, Health, 1f, 1f, 1f, 0, Level, GlobalProperties.ToString(), "Idle||"));

        Room.SendSyncEvent(AISyncEventHelper.AIDo(Id, Room.Time,
            new Vector3 { x = Position.X + SpawningOffsetX, y = Position.Y + SpawningOffsetY, z = Position.Z },
            1.0f, BehaviorList.IndexOf(StateTypes.Unknown), string.Empty, Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY,
            Generic.Patrol_ForceDirectionX, false));

        var spawn = new Spawn_SyncEvent(Id, Room.Time, _spawnedEntityCount);
        Room.SendSyncEvent(spawn);

        _spawnedEntityId = $"{Id}_{_spawnedEntityCount}";

        _arena?.ArenaEntities.Add(_spawnedEntityId);

        TimerThread.DelayCall(DelayedSpawnData, string.Empty, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
    }

    private void DelayedSpawnData(object _)
    {
        //Set all component data
        var newEntity = new List<BaseComponent>
        {
            Global,
            Generic,
            Status,
            EnemyController,
            Hazard

        };
        Room.AddEntity(_spawnedEntityId, newEntity);
        foreach (var component in newEntity)
        {
            component.InitializeComponent();
            Room.KilledObjects.Remove(component.Id);
        }

        //Fix some things before setting the enemy
        Hazard.SetId(_spawnedEntityId);
        Generic.SetPatrolRange(PatrolDistance);

        Room.Enemies.Add(_spawnedEntityId, SetEnemy(_spawnedEntityCount));
        _nextSpawnRequestTime = 0;
    }

    private BehaviorEnemy SetEnemy(int index)
    {
        var enemy = Room.GenerateEntityFromName(PrefabNameToSpawn1, _spawnedEntityId, EnemyController, Services, ServerRConfig);

        if (enemy is not BehaviorEnemy bEnemy)
            return null;

        LinkedEnemies.Add(index, bEnemy);

        bEnemy.Initialize();

        return bEnemy;
    }

    private bool IsPlayerNearby(float radius)
    {
        foreach (var player in Room.Players)
        {
            var pos = player.Value.TempData.Position;
            if (Position.X - radius < pos.X && pos.X < Position.X + radius &&
                   Position.Y - radius < pos.Y && pos.Y < Position.Y + radius)
                return true;
        }
        return false;
    }

    public void NotifyEnemyDefeat(int id) => LinkedEnemies.Remove(id);
}
