using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class GlobalCounterInteractableControllerComp : Component<GlobalCounterInteractableController>
{

    public int Interactions;

    public override void InitializeComponent()
    {
    }

    //public override void RunSyncedEvent(SyncEvent syncEvent, Player player) { }
}
