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

    public ITriggerComp triggerable;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        foreach (var triggerableComp in Room.GetEntitiesFromId<ITriggerComp>(ArenaObjectId))
            triggerable = triggerableComp;

        if (!IsEnabled)
            IsEnabled = triggerable != null;
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Id == "5664") // Temporary while blue arenas are in progress
        {
            player.CheckObjective(ObjectiveEnum.Score, ArenaObjectId, PrefabName, 1);
            return;
        }

        if (triggerable == null)
        {
            Logger.LogError("Could not find trigger with id {Id}!", ArenaObjectId);
            return;
        }

        if (triggerable.IsActive())
        {
            Logger.LogError("Trigger {Id} is already running!", ArenaObjectId);
            return;
        }

        base.RunSyncedEvent(syncEvent, player);
    }

    // Should be redone to connect to ArenaSwitchBase
    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (isActive)
        {
            triggerable.AddPhysicalInteractor(player.GameObjectId);
            triggerable.RunTrigger(player);
        }
    }
}
