using Server.Base.Network;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities;

public class BouncerControllerModel : SyncedEntity<BouncerController>
{
    public float BounceVerticalVelocity => EntityData.BounceVerticalVelocity;
    public float BounceHorizontalVelocity => EntityData.BounceHorizontalVelocity;
    public float BounceHorizontalInputVelocity => EntityData.BounceHorizontalInputVelocity;

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var bouncer = new Bouncer_SyncEvent(syncEvent);
        Room.SendSyncEvent(new Bouncer_SyncEvent(bouncer.TargetID, bouncer.TriggerTime, false), player);

        netState.SendSyncEventToPlayer(bouncer);
    }
}
