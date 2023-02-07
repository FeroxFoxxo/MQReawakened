using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities;

public class BouncerControllerModel : SynchronizedEntity<BouncerController>
{
    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var bouncer = new Bouncer_SyncEvent(syncEvent);
        var player = netState.Get<Player>();
        Level.SendSyncEvent(new Bouncer_SyncEvent(bouncer.TargetID, bouncer.TriggerTime, false), player);
        Level.SendSyncEventToPlayer(bouncer, netState);
    }
}
