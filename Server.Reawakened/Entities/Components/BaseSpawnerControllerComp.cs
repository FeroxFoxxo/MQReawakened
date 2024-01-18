using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System.ComponentModel;
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
    public override void InitializeComponent()
    {
    }
    public override void Update()
    {
    }
    public void Spawn()
    {
        var spawn = new Spawn_SyncEvent(Id.ToString(), Room.Time, 1);
        Room.SendSyncEvent(spawn);
    }
}
