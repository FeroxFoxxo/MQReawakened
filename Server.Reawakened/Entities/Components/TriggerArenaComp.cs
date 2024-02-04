using A2m.Server;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using SmartFoxClientAPI.Data;

namespace Server.Reawakened.Entities.Components;

public class TriggerArenaComp : TriggerStatueComp<TriggerArena>
{
    private float _timer;
    private float _minClearTime;
    private bool _hasStarted;

    public List<string> ArenaEntities;

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        ArenaEntities = [];
        _hasStarted = false;
    }

    public override object[] GetInitData(Player player) => [ -1 ];

    public override void Update()
    {
        if (_hasStarted)
            if (Room.Time >= _timer || !ArenaEntities.Any(Room.Entities.ContainsKey) && Room.Time >= _minClearTime)
                Trigger(Room.Players.FirstOrDefault().Value, false);
    }

    public override void Triggered(Player _, bool isSuccess, bool isActive)
    {
        if (IsActive)
        {
            foreach (var entity in Triggers.Where(x => x.Value == Enums.TriggerType.Activate).Select(x => x.Key))
            {
                if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger) && int.Parse(entity) > 0)
                {
                    foreach (var component in foundTrigger)
                    {
                        if (component is BaseSpawnerControllerComp spawner)
                        {
                            // Add "PF_CRS_SpawnerBoss01" to config on cleanup
                            if (spawner.PrefabName != "PF_CRS_SpawnerBoss01")
                                ArenaEntities.Add(entity.ToString());

                            // A special surprise tool that'll help us later!
                            //var spawn = new Spawn_SyncEvent(spawner.Id, player.Room.Time, 1);
                            //player.Room.SendSyncEvent(spawn);
                        }
                    }
                }
            }

            _timer = Room.Time + ActiveDuration;

            //Add to ServerRConfig eventually. This exists to stop the arena from regenerating if the spawners are defeated before it has finished initializing
            _minClearTime = Room.Time + 12;
        }
        else
        {
            //Trigger rewarded entities on win and shut down Arena
            if (!ArenaEntities.Any(Room.Entities.ContainsKey) && Room.Time >= _minClearTime)
            {
                foreach (var entity in TriggeredRewards)
                    if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger))
                        foreach (var component in foundTrigger)
                            if (component is TriggerReceiverComp trigger)
                                trigger.Trigger(true);

                foreach (var player in Room.Players.Values)
                    player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1);
            }
            else
                foreach (var player in Room.Players.Values)
                    RemovePhysicalInteractor(player.GameObjectId);
        }

        _hasStarted = isActive;
    }
}
