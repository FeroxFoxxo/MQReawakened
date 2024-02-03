using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using SmartFoxClientAPI.Data;

namespace Server.Reawakened.Entities.Components;

public class TriggerArenaComp : TriggerStatueComp<TriggerArena>
{
    private float _timer;
    private float _minClearTime;

    public readonly List<string> ArenaEntities = [];

    public override void Triggered(Player _, bool isSuccess, bool isActive)
    {
        if (IsActive)
        {
            foreach (var entity in TriggeredEntities)
            {
                if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger) && entity != 0)
                {
                    foreach (var component in foundTrigger)
                    {
                        if (component is TriggerReceiverComp trigger)
                            trigger.Trigger(true);
                        else if (component is BaseSpawnerControllerComp spawner)
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
            //Shut down all active entities on stop
            foreach (var entity in TriggeredEntities)
                if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger))
                    foreach (var component in foundTrigger)
                        if (component is TriggerReceiverComp trigger)
                            trigger.Trigger(false);

            var hasWon = false;

            if (Room.Time >= _timer)
                hasWon = false;
            else if (!ArenaEntities.Any(Room.Entities.ContainsKey) && Room.Time >= _minClearTime)
                hasWon = true;
            else
                Logger.LogError("Unkown arena condition for {Id}", Id);

            foreach (var player in Room.Players.Values)
                Room.SendSyncEvent(new Trigger_SyncEvent(Id, Room.Time, hasWon, player.GameObjectId, false));

            //Trigger rewarded entities on win and shut down Arena
            if (hasWon)
            {
                foreach (var entity in TriggeredRewards)
                    if (Room.Entities.TryGetValue(entity.ToString(), out var foundTrigger))
                        foreach (var component in foundTrigger)
                            if (component is TriggerReceiverComp trigger)
                                trigger.Trigger(true);

                foreach (var player in Room.Players.Values)
                    player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1);
            }
        }
    }
}
