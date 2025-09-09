using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Rooms;
using SmartFoxClientAPI.Data;
using Microsoft.Extensions.Logging;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerArenaComp : BaseTriggerStatueComp<TriggerArena>
{
    private float _timer;
    private float _minClearTime;
    private List<BaseSpawnerControllerComp> _spawners;

    public List<string> ArenaEntities;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        ArenaEntities = [];
        _spawners = [];
    }

    public override void DelayedComponentInitialization()
    {
        foreach (var entity in Triggers.Where(x => x.Value == TriggerType.Activate).Select(x => x.Key))
            if (int.Parse(entity) > 0)
                foreach (var spawner in Room.GetEntitiesFromId<BaseSpawnerControllerComp>(entity))
                {
                    spawner.SetArena(this);
                    spawner.SetActive(false);
                }
    }

    public override ArenaStatus GetArenaStatus()
    {
        var outStatus = Status == ArenaStatus.Complete ? ArenaStatus.Complete : ArenaStatus.Incomplete;

        if (HasStarted)
        {
            try
            {
                if (ArenaEntities != null && ArenaEntities.Count > 0)
                    ArenaEntities = ArenaEntities.Where(e => !Room.IsObjectKilled(e)).ToList();
            }
            catch { }

            var allSpawnersDead = true;
            if (_spawners != null && _spawners.Count > 0)
            {
                allSpawnersDead = _spawners.All(s => s != null && Room.IsObjectKilled(s.Id.ToString()));
            }
            else if (ArenaEntities != null && ArenaEntities.Count > 0)
            {
                allSpawnersDead = ArenaEntities.All(Room.IsObjectKilled);
            }

            var allSpawnedEnemiesDead = true;
            if (_spawners != null && _spawners.Count > 0)
            {
                foreach (var sp in _spawners)
                {
                    if (sp == null)
                        continue;

                    if (sp.LinkedEnemies != null && sp.LinkedEnemies.Count > 0)
                    {
                        var anyAlive = sp.LinkedEnemies.Keys.Any(eid => !Room.IsObjectKilled(eid));
                        if (anyAlive)
                        {
                            allSpawnedEnemiesDead = false;
                            break;
                        }
                    }
                }
            }

            try
            {
                var arenaTotal = ArenaEntities?.Count ?? 0;
                var arenaAlive = ArenaEntities?.Count(e => !Room.IsObjectKilled(e)) ?? 0;
                var spawnerCount = _spawners?.Count ?? 0;
                var spawnersDeadCount = _spawners?.Count(s => s == null || Room.IsObjectKilled(s.Id.ToString())) ?? 0;
                var linkedAlive = _spawners?.Sum(s => s.LinkedEnemies?.Count(e => !Room.IsObjectKilled(e.Key)) ?? 0) ?? 0;

                Room?.Logger?.LogDebug("ArenaStatus debug: spawners {SpawnerCount} (dead {SpawnersDead}), arenaEntities total {ArenaTotal} (alive {ArenaAlive}), linkedEnemies alive {LinkedAlive}, time {Time} minClear {MinClear} timer {Timer}",
                    spawnerCount, spawnersDeadCount, arenaTotal, arenaAlive, linkedAlive, Room.Time, _minClearTime, _timer);
            }
            catch { }

            if (allSpawnersDead && allSpawnedEnemiesDead && Room.Time >= _minClearTime)
                outStatus = ArenaStatus.Win;
            else if (Room.Time >= _timer)
                outStatus = ArenaStatus.Lose;
        }

        return outStatus;
    }

    public override void StartArena()
    {
        foreach (var entity in Triggers.Where(x => x.Value == TriggerType.Activate).Select(x => x.Key))
        {
            if (int.Parse(entity) <= 0)
                continue;

            foreach (var spawner in Room.GetEntitiesFromId<BaseSpawnerControllerComp>(entity))
            {
                if (spawner.SpawnCycleCount > 1)
                    ArenaEntities.Add(entity.ToString());

                spawner.Spawn(this);
                _spawners.Add(spawner);
            }
        }

        _timer = Room.Time + ActiveDuration;

        _minClearTime = Room.Time + 5;
    }


    public override void ArenaSuccess()
    {
        base.ArenaSuccess();

        foreach (var spawner in _spawners)
        {
            spawner.Despawn();
            spawner.Destroy();
        }
    }

    public override void ArenaFailure()
    {
        base.ArenaFailure();

        foreach (var spawner in _spawners)
        {
            spawner.Despawn();
            spawner.Revive();
        }

        ArenaEntities.Clear();
        _spawners.Clear();
    }
}
