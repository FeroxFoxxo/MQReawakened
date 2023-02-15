using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

internal class ChestControllerEntity : SyncedEntity<ChestController>
{
    public bool Collected;

    public Random Random { get; set; }

    public override object[] GetInitData(NetState netState) => new object[] { Collected ? 0 : 1 };

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        if (Collected)
            return;

        Collected = true;

        var player = netState.Get<Player>();

        var bananas = Random.Next(10, 100);
        player.AddBananas(netState, bananas);

        var trig = new Trigger_SyncEvent(Id.ToString(), Level.Time, true, player.PlayerId.ToString(), true)
            {
                EventDataList =
                {
                    [0] = bananas
                }
            };

        Level.SendSyncEvent(trig);

        var rec = new TriggerReceiver_SyncEvent(Id.ToString(), Level.Time, player.PlayerId.ToString(), true, 1f);
        
        Level.SendSyncEvent(rec);
    }
}
