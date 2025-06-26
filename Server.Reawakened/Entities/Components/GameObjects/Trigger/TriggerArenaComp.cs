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
            if (ArenaEntities.All(Room.IsObjectKilled) && Room.Time >= _minClearTime)
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

        //Failsafe to prevent respawn issues when arena is defeated too quickly
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
