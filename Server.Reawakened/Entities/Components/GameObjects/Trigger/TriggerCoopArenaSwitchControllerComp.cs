using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerCoopArenaSwitchControllerComp : BaseTriggerCoopController<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public ILogger<TriggerCoopArenaSwitchControllerComp> Logger { get; set; }

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
        if (triggerable == null)
        {
            Logger.LogError("Could not find trigger with id {Id}!", ArenaObjectId);

            // Temporary while blue arenas are in progress
            player.CheckObjective(ObjectiveEnum.Score, ArenaObjectId, PrefabName, 1, QuestCatalog);

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
            triggerable.AddPhysicalInteractor(player, player.GameObjectId);
            triggerable.RunTrigger(player);
        }
    }
}
