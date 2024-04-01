using A2m.Server;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using SmartFoxClientAPI.Data;

namespace Server.Reawakened.Entities.Components;

public class TriggerArenaComp : BaseTriggerStatueComp<TriggerArena>
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

    public override object[] GetInitData(Player player) => [-1];

    public override void Update()
    {
        if (_hasStarted)
            if (Room.Time >= _timer || ArenaEntities.All(Room.IsObjectKilled) && Room.Time >= _minClearTime)
                Trigger(Room.Players.FirstOrDefault().Value, false);
    }

    public override void Triggered(Player _, bool isSuccess, bool isActive)
    {
        if (IsActive)
        {
            foreach (var entity in Triggers.Where(x => x.Value == Enums.TriggerType.Activate).Select(x => x.Key))
            {
                if (int.Parse(entity) <= 0)
                    continue;

                foreach (var spawner in Room.GetEntitiesFromId<BaseSpawnerControllerComp>(entity))
                {
                    // Add "PF_CRS_SpawnerBoss01" to ServerRConfig on cleanup
                    if (spawner.PrefabName != "PF_CRS_SpawnerBoss01")
                        ArenaEntities.Add(entity.ToString());

                    // A special surprise tool that'll help us later!
                    spawner.Spawn(this);
                }
            }

            _timer = Room.Time + ActiveDuration;

            //Add to ServerRConfig eventually. This exists to stop the arena from regenerating if the spawners are defeated before it has finished initializing
            _minClearTime = Room.Time + 12;
        }
        else
        {
            //Trigger rewarded entities on win and shut down Arena
            if (ArenaEntities.All(Room.IsObjectKilled) && Room.Time >= _minClearTime)
            {
                foreach (var entity in TriggeredRewards)
                    foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(entity.ToString()))
                        trigger.Trigger(true);

                foreach (var player in Room.Players.Values)
                    player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1, QuestCatalog);
            }
            else
                foreach (var player in Room.Players.Values)
                    RemovePhysicalInteractor(player.GameObjectId);
        }

        _hasStarted = isActive;
    }
}
