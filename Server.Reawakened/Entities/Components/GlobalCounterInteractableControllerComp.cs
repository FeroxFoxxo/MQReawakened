using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class GlobalCounterInteractableControllerComp : Component<GlobalCounterInteractableController>
{
    public int Interactions { get; set; }
}
