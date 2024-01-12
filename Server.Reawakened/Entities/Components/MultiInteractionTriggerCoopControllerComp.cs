using A2m.Server;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;

public class MultiInteractionTriggerCoopControllerComp : TriggerCoopControllerComp<MultiInteractionTriggerCoopController>
{
    public InternalObjective ObjectiveCatalog { get; set; }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (isActive)
            player.CheckObjective(ObjectiveEnum.Goto, Id, PrefabName, 1);
    }
}
