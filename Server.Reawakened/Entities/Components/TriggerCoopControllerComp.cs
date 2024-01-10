using A2m.Server;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities.Components;

public class TriggerCoopControllerComp : TriggerCoopControllerComp<TriggerCoopController>
{
    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (isActive)
            player.CheckObjective(ObjectiveEnum.Goto, Id, PrefabName, 1);
    }
}
