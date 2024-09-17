using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Rooms;
using SmartFoxClientAPI.Data;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerProtectionArenaComp : BaseTriggerStatueComp<TriggerProtectionArena>
{
    public int ProtectObjectID => ComponentData.ProtectObjectID;

    private float _defeatedCount;
    private float _maxDefeatsRequired;
    private BreakableEventControllerComp _protectObject;
    private List<BaseSpawnerControllerComp> _spawners;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        _defeatedCount = 0;
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
                    _maxDefeatsRequired += spawner.SpawnCycleCount;
                }

        foreach (var breakableObj in Room.GetEntitiesFromId<BreakableEventControllerComp>(ProtectObjectID.ToString()))
            _protectObject = breakableObj;
    }

    public override ArenaStatus GetArenaStatus()
    {
        var outStatus = Status == ArenaStatus.Complete ? ArenaStatus.Complete : ArenaStatus.Incomplete;
        if (HasStarted)
            outStatus = _defeatedCount >= _maxDefeatsRequired ? ArenaStatus.Win : ArenaStatus.Incomplete;

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
                spawner.Spawn(this);
                _spawners.Add(spawner);
            }
        }
    }

    public void AddDefeat() => _defeatedCount += 1;

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

        _protectObject.Respawn();
        _defeatedCount = 0;
        _spawners.Clear();
    }
}
