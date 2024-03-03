using Server.Base.Core.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
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

    public int Health;
    public int Level;
    private GlobalProperties _globalProps;

    public override void InitializeComponent()
    {
        //For global properties, make a per-enemy initializer for this in the enemy resource (or add a generic GlobalProperties to SeverRConfig)
        _globalProps = new GlobalProperties(false, 0, 2, 0, 0, 0, 1.5f, 8, 0, 0, "Generic", string.Empty, false, false, 0);

        //Everything here is temporary until I add that world statistics xml thingy
        Level = 1;
        Health = (int)ComponentData.GetField("_currentHealth");

        Room.SendSyncEvent(InitializeAIInit());
        Room.SendSyncEvent(InitializeAIDo());
    }

    public override void Update()
    {
    }

    public AIInit_SyncEvent InitializeAIInit()
    {
        //Add way to consult InternalEnemyResources.xml here
        var behavior = GetBehaviour();

        var aiInit = new AIInit_SyncEvent(Id + "_1", Room.Time, Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY, Position.Z, Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY,
            0, Health, Health, 1, 1, 1, 0, Level, _globalProps.ToString(), behavior);

        aiInit.EventDataList[2] = Position.X + SpawningOffsetX;
        aiInit.EventDataList[3] = Position.Y + SpawningOffsetY;
        aiInit.EventDataList[4] = Position.Z;

        return aiInit;
    }

    public AIDo_SyncEvent InitializeAIDo()
    {
        //Add way to consult InternalEnemyResources.xml here
        var aiDo = new AIDo_SyncEvent(new SyncEvent(Id + "_1", SyncEvent.EventType.AIDo, Room.Time));

        aiDo.EventDataList.Clear();
        aiDo.EventDataList.Add(Position.X + SpawningOffsetX);
        aiDo.EventDataList.Add(Position.Y + SpawningOffsetY);
        aiDo.EventDataList.Add(1.0);
        aiDo.EventDataList.Add(1);
        aiDo.EventDataList.Add(string.Empty);
        aiDo.EventDataList.Add(PatrolDistance.x);
        aiDo.EventDataList.Add(PatrolDistance.y);
        aiDo.EventDataList.Add(0);
        // 0 for false, 1 for true.
        aiDo.EventDataList.Add(0);

        return aiDo;
    }

    public void Spawn()
    {
        var spawn = new Spawn_SyncEvent(Id.ToString(), Room.Time, 1);

        Room.SendSyncEvent(spawn);

        //Add way to consult InternalEnemyResources.xml here
        var behavior = GetBehaviour();

        var aiInit = new AIInit_SyncEvent(Id + "_1", Room.Time, Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY, Position.Z, Position.X + SpawningOffsetX, Position.Y + SpawningOffsetY,
            0, Health, Health, 1, 1, 1, 0, Level, _globalProps.ToString(), behavior);
        
        aiInit.EventDataList[2] = Position.X + SpawningOffsetX;
        aiInit.EventDataList[3] = Position.Y + SpawningOffsetY;
        aiInit.EventDataList[4] = Position.Z;

        Room.SendSyncEvent(aiInit);
    }

    public string GetBehaviour()
    {
        var sSb = new SeparatedStringBuilder('|');

        sSb.Append("Idle");
        sSb.Append(string.Empty);

        sSb.Append("Patrol");
        var pSb = new SeparatedStringBuilder(';');
        pSb.Append(1.8);
        pSb.Append(0);
        pSb.Append(3);
        pSb.Append(PatrolDistance.x);
        pSb.Append(PatrolDistance.y);
        pSb.Append(0);
        pSb.Append(0);
        sSb.Append(pSb.ToString());

        sSb.Append(string.Empty);

        return sSb.ToString();
    }
}
