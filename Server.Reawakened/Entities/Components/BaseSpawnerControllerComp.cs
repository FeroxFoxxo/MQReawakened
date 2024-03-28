using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Enemies.EnemyAI;
using Server.Reawakened.Entities.Enemies.EnemyAI.BehaviorEnemies;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.BundlesInternal;
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
        GlobalProperties = new GlobalProperties(true, 0, 0, 0, 0, 0, 0, 0, 0, 0, "Generic", "", false, false, 0);

        BehaviorList = EnemyInfoXml.GetBehaviorsByName(PrefabNameToSpawn1);
        LinkedEnemies = new Dictionary<int, BehaviorEnemy>();

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
        Room.SendSyncEvent(new AIInit_SyncEvent(Id, Room.Time, 0, 0, 0, 0, 0, Generic.Patrol_InitialProgressRatio,
        Health, Health, 1f, 1f, 1f, 0, Level, GlobalProperties.ToString(), "Idle||"));

        var aiDo = new AIDo_SyncEvent(new SyncEvent(Id, SyncEvent.EventType.AIDo, Room.Time));

        aiDo.EventDataList.Add(0);
        aiDo.EventDataList.Add(0);
        aiDo.EventDataList.Add(1f);
        aiDo.EventDataList.Add(0);
        aiDo.EventDataList.Add("");
        aiDo.EventDataList.Add(0);
        aiDo.EventDataList.Add(0);
        aiDo.EventDataList.Add(0);
        aiDo.EventDataList.Add(0);

        Room.SendSyncEvent(aiDo);

        var spawn = new Spawn_SyncEvent(Id, Room.Time, _spawnedEntityCount);
        Room.SendSyncEvent(spawn);

        _spawnedEntityId = $"{Id}_{_spawnedEntityCount}";

        _arena?.ArenaEntities.Add(_spawnedEntityId);

        TimerThread.DelayCall(DelayedSpawnData, "", TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
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
        switch (PrefabNameToSpawn1)
        {
            case string bird when bird.Contains(ServerRConfig.EnemyNameSearch[0]):
                LinkedEnemies.Add(index, new EnemyBird(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string fish when fish.Contains(ServerRConfig.EnemyNameSearch[1]):
                LinkedEnemies.Add(index, new EnemyFish(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string spider when spider.Contains(ServerRConfig.EnemyNameSearch[2]):
                LinkedEnemies.Add(index, new EnemySpider(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string bathog when bathog.Contains(ServerRConfig.EnemyNameSearch[3]):
                LinkedEnemies.Add(index, new EnemyBathog(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string bomber when bomber.Contains(ServerRConfig.EnemyNameSearch[4]):
                LinkedEnemies.Add(index, new EnemyBomber(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string crawler when crawler.Contains(ServerRConfig.EnemyNameSearch[5]):
                LinkedEnemies.Add(index, new EnemyCrawler(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string dragon when dragon.Contains(ServerRConfig.EnemyNameSearch[6]):
                LinkedEnemies.Add(index, new EnemyDragon(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string grenadier when grenadier.Contains(ServerRConfig.EnemyNameSearch[7]):
                LinkedEnemies.Add(index, new EnemyGrenadier(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string orchid when orchid.Contains(ServerRConfig.EnemyNameSearch[8]):
                LinkedEnemies.Add(index, new EnemyOrchid(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string pincer when pincer.Contains(ServerRConfig.EnemyNameSearch[9]):
                LinkedEnemies.Add(index, new EnemyPincer(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string stomper when stomper.Contains(ServerRConfig.EnemyNameSearch[10]):
                LinkedEnemies.Add(index, new EnemyStomper(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
            case string vespid when vespid.Contains(ServerRConfig.EnemyNameSearch[11]):
                LinkedEnemies.Add(index, new EnemyVespid(Room, _spawnedEntityId, PrefabNameToSpawn1, EnemyController, Services));
                break;
        }

        LinkedEnemies.TryGetValue(index, out var enemy);
        enemy.Initialize();
        return enemy;
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
