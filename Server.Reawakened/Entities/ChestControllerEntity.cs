using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

internal class ChestControllerEntity : SyncedEntity<ChestController>
{
    public bool Collected = false;

    public override string[] GetInitData(NetState netState) => new[] { Collected ? "0" : "1" };

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        Level.SendSyncEvent(syncEvent);
        netState.SendSyncEventToPlayer(syncEvent);

        var rec = new TriggerReceiver_SyncEvent(Id.ToString(), Level.Time, player.PlayerId.ToString(), true, 1f);
        Level.SendSyncEvent(rec);
        netState.SendSyncEventToPlayer(rec);
    }
}
