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

        if (Room.Entities.TryGetValue(ArenaObjectId, out var foundEntity))
            foreach (var component in foundEntity)
                if (component is ITriggerComp triggerableComp)
                    triggerable = triggerableComp;

        if (!IsEnabled)
            IsEnabled = triggerable != null;
    }

    // Should be redone to connect to ArenaSwitchBase
    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (isActive)
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

            triggerable.AddPhysicalInteractor(player.GameObjectId);
            triggerable.RunTrigger(player);

            IsActive = false;
            IsEnabled = false;
        }
    }
}
