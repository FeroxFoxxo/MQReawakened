using A2m.Server;
using Server.Reawakened.Entities.Components.GameObjects.Spawners;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using SmartFoxClientAPI.Data;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerArenaComp : BaseTriggerStatueComp<TriggerArena>
{
    private float _timer;
    private float _minClearTime;
    private bool _hasStarted;
    private bool _cleared;
    private List<BaseSpawnerControllerComp> _spawners;

    public List<string> ArenaEntities;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        ArenaEntities = [];
        _spawners = [];
        _hasStarted = false;
        _cleared = false;
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

    public override void SendDelayedData(Player player)
    {
        var trigger = new Trigger_SyncEvent(Id.ToString(), Room.Time, false, "now", false);
        if (_cleared)
            new Trigger_SyncEvent(Id.ToString(), Room.Time, true, "now", false);
        else if (!_cleared && _hasStarted)
            new Trigger_SyncEvent(Id.ToString(), Room.Time, true, "now", true);

        player.SendSyncEventToPlayer(trigger);
    }

    public override object[] GetInitData(Player player) => [-1];

    public override void Update()
    {
        if (_hasStarted)
        {
            if (ArenaEntities.All(Room.IsObjectKilled) && Room.Time >= _minClearTime)
                ArenaSuccess();
            else if (Room.Time >= _timer)
                ArenaFailure();
        }
    }

    public override void Triggered(Player origin, bool isSuccess, bool isActive)
    {
        if (IsActive)
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

            var players = Room.GetPlayers();
            foreach (var player in players)
                player.TempData.CurrentArena = this;
        }
        else
        {
            var players = Room.GetPlayers();

            //Trigger rewarded entities on win and shut down Arena
            if (ArenaEntities.All(Room.IsObjectKilled) && Room.Time >= _minClearTime)
            {
                foreach (var entity in TriggeredRewards)
                    foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(entity.ToString()))
                        trigger.Trigger(true, origin.GameObjectId);

                foreach (var player in players)
                {
                    player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, QuestCatalog);
                    player.Character.Write.SpawnPointId = Id;
                }
            }
            else
                foreach (var player in players)
                    RemovePhysicalInteractor(player, player.GameObjectId);
        }

        _hasStarted = isActive;
    }

    private void ArenaSuccess()
    {
        var players = Room.GetPlayers();
        Trigger(players.FirstOrDefault(), true, false);
        foreach (var player in players)
            player.TempData.CurrentArena = null;

        foreach (var spawner in _spawners)
        {
            spawner.Despawn();
            spawner.Destroy();
        }
        _cleared = true;
    }

    private void ArenaFailure()
    {
        var players = Room.GetPlayers();
        Trigger(players.FirstOrDefault(), false, false);
        foreach (var player in players)
            player.TempData.CurrentArena = null;

        foreach (var spawner in _spawners)
        {
            spawner.Despawn();
            spawner.Revive();
        }

        ArenaEntities.Clear();
        _spawners.Clear();
    }
}
