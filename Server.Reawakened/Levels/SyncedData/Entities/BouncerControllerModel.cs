using Server.Base.Network;
using Server.Reawakened.Levels.SyncedData.Abstractions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Levels.SyncedData.Entities;

public class BouncerControllerModel : SynchronizedEntity<BouncerController>
{
    public BouncerControllerModel(StoredEntityModel storedEntity,
        BouncerController entityData) : base(storedEntity, entityData)
    {
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var bouncer = new Bouncer_SyncEvent(syncEvent);
        var player = netState.Get<Player>();
        Level.SendSyncEvent(new Bouncer_SyncEvent(bouncer.TargetID, bouncer.TriggerTime, false), player);
        Level.SendSyncEventToPlayer(bouncer, netState);
    }
}
