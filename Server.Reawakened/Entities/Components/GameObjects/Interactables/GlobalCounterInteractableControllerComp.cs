using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Interactables;

public class GlobalCounterInteractableControllerComp : Component<GlobalCounterInteractableController>
{
    public int Interactions { get; set; }

    public override object[] GetInitData(Player player) => [];
}
