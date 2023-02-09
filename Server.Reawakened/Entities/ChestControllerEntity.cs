using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

internal class ChestControllerEntity : SyncedEntity<ChestController>
{
    public bool Collected = false;

    private Random _rnd;

    public override void InitializeEntity() => _rnd = new Random();

    public override string[] GetInitData(NetState netState) => new[] { Collected ? "0" : "1" };

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        if (Collected) return;

        Collected = true;

        var player = netState.Get<Player>();

        var bananas = _rnd.Next(10, 100);
        player.AddBananas(netState, bananas);

        var trig = new Trigger_SyncEvent(Id.ToString(), Level.Time, true, player.PlayerId.ToString(), true);
        trig.EventDataList[0] = bananas;

        Level.SendSyncEvent(trig);
        netState.SendSyncEventToPlayer(trig);

        var rec = new TriggerReceiver_SyncEvent(Id.ToString(), Level.Time, player.PlayerId.ToString(), true, 1f);
        Level.SendSyncEvent(rec);
        netState.SendSyncEventToPlayer(rec);
    }
}
