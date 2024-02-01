using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Entities.Stats;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using SmartFoxClientAPI.Data;
using System.Text;

namespace Server.Reawakened.Entities.Components;

public class TriggerArenaComp : TriggerCoopControllerComp<TriggerArena>
{
    public bool Active;
    public int[] TriggeredEntities;
    public int[] TriggeredRewards;
    public int Target09LevelEditorId => ComponentData.Target09LevelEditorID;
    public int Target10LevelEditorId => ComponentData.Target10LevelEditorID;
    public int Target11LevelEditorId => ComponentData.Target11LevelEditorID;
    public int Target12LevelEditorId => ComponentData.Target12LevelEditorID;

    public int TargetReward01LevelEditorID => ComponentData.TargetReward01LevelEditorID;
    public int TargetReward02LevelEditorID => ComponentData.TargetReward02LevelEditorID;
    public int TargetReward03LevelEditorID => ComponentData.TargetReward03LevelEditorID;
    public int TargetReward04LevelEditorID => ComponentData.TargetReward04LevelEditorID;

    private float _timer;
    private float _minClearTime;
    private List<string> _arenaEntities;

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        Active = false;

        TriggeredEntities = [
            TargetLevelEditorId,
            Target02LevelEditorId,
            Target03LevelEditorId,
            Target04LevelEditorId,
            Target05LevelEditorId,
            Target06LevelEditorId,
            Target07LevelEditorId,
            Target08LevelEditorId,
            Target09LevelEditorId,
            Target10LevelEditorId,
            Target11LevelEditorId,
            Target12LevelEditorId
        ];
        TriggeredRewards = [
            TargetReward01LevelEditorID,
            TargetReward02LevelEditorID,
            TargetReward03LevelEditorID,
            TargetReward04LevelEditorID
        ];

        _arenaEntities = [];
    }

    public override void Update()
    {
        if(Active)
        {
            ActiveArena();
        }

    }

    public void StartArena(Player player)
    {
        if (!Active)
        {
            foreach (var entity in TriggeredEntities)
            {
                if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger) && !(entity == null || entity == 0))
                {
                    foreach (var component in foundTrigger)
                    {
                        if (component is TriggerReceiverComp trigger)
                            trigger.Trigger(true);
                        // Add "PF_CRS_SpawnerBoss01" to config on cleanup
                        if (component is BaseSpawnerControllerComp spawner)
                        {
                            if (component.PrefabName != "PF_CRS_SpawnerBoss01")
                            {
                                _arenaEntities.Add(entity.ToString());
                            }

                            // A Special surprise tool that'll help us later!
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
        Active = true;
    }

    public void StopArena(bool win)
    {
        Active = false;

        //Shut down all active entities on stop
        foreach (var entity in TriggeredEntities)
        {
            if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger))
            {
                foreach (var component in foundTrigger)
                {
                    if (component is TriggerReceiverComp trigger)
                        trigger.Trigger(false);
                }
            }
        }

        //Trigger rewarded entities on win and shut down Arena
        if (win)
        {
            Room.SendSyncEvent(new Trigger_SyncEvent(Id, Room.Time, true, GrabAnyPlayer(), false));

            foreach (var entity in TriggeredRewards)
            {
                if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger))
                {
                    foreach (var component in foundTrigger)
                    {
                        if (component is TriggerReceiverComp trigger)
                            trigger.Trigger(true);
                    }
                }
            }

            foreach (var player in Room.Players.Values)
                player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1);
        }
        else
            Room.SendSyncEvent(new Trigger_SyncEvent(Id, Room.Time, false, GrabAnyPlayer(), false));
    }

    private void ActiveArena()
    {
        if (Room.Time >= _timer)
            StopArena(false);
        else if (IsArenaComplete() && Room.Time >= _minClearTime)
            StopArena(true);
    }

    private string GrabAnyPlayer()
    {
        foreach (var player in Room.Players)
        {
            return player.Value.GameObjectId;
        }
        return "0";
    }

    private bool IsArenaComplete()
    {
        foreach (var entity in _arenaEntities)
        {
            if (Room.Entities.TryGetValue(entity.ToString(), out var x))
            {
                return false;
            }
        }
        return true;
    }
}
