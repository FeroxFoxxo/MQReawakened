using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class BouncerControllerComp : Component<BouncerController>
{
    public float BounceVerticalVelocity => ComponentData.BounceVerticalVelocity;
    public float BounceHorizontalVelocity => ComponentData.BounceHorizontalVelocity;
    public float BounceHorizontalInputVelocity => ComponentData.BounceHorizontalInputVelocity;

    public override void InitializeComponent() => base.InitializeComponent();

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var bouncer = new Bouncer_SyncEvent(syncEvent);
        Room.SendSyncEvent(new Bouncer_SyncEvent(bouncer.TargetID, bouncer.TriggerTime, false), player);
        player.SendSyncEventToPlayer(bouncer);
    }
}
