using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;

public class MultiInteractionTriggerCoopControllerComp : TriggerCoopControllerComp<MultiInteractionTriggerCoopController>
{
    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (isActive)
            player.CheckObjective(ObjectiveEnum.Goto, Id, PrefabName, 1);
    }
}
