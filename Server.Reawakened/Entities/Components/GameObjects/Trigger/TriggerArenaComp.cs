using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Rooms;
using SmartFoxClientAPI.Data;

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
