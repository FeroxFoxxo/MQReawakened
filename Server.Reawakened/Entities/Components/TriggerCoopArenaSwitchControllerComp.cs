using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Entities.Components;

public class TriggerCoopArenaSwitchControllerComp : TriggerCoopControllerComp<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public DatabaseContainer DatabaseContainer { get; set; }
    public new ILogger<TriggerCoopArenaSwitchControllerComp> Logger { get; set; }

    public IStatueComp Statue;

    public override void InitializeComponent()
    {
        if (Room.Entities.TryGetValue(ArenaObjectId, out var foundEntity))
            foreach (var component in foundEntity)
                if (component is IStatueComp statueComp)
                    Statue = statueComp;
    }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (Statue == null)
        {
            Logger.LogError("Could not find statue with id {Id}!", Id);
            return;
        }

        if (Statue.CurrentPhysicalInteractors.Count > 0)
            return;

        foreach (var playerId in CurrentPhysicalInteractors)
            if (playerId != player.GameObjectId)
                Statue.CurrentPhysicalInteractors.Add(playerId);

        var statueSyncEvent = new Trigger_SyncEvent(ArenaObjectId, Room.Time, true, player.GameObjectId, isActive);
        Statue.RunSyncedEvent(statueSyncEvent, player);

        if (isActive)
        {
            if (Id == "5664") // Temporary while blue arenas are in progress
            {
                player.CheckObjective(ObjectiveEnum.Score, ArenaObjectId, PrefabName, 1);
                return;
            }

            foreach (var playerId in CurrentPhysicalInteractors)
                if (playerId != player.GameObjectId)
                    Statue.CurrentPhysicalInteractors.Remove(playerId);

            IsActive = false;

            var triggerSyncEvent = new Trigger_SyncEvent(Id, Room.Time, true, player.GameObjectId, IsActive);
            RunSyncedEvent(triggerSyncEvent, player);

            IsEnabled = false;
        }
    }
}
