using Server.Base.Network;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class ChestControllerEntity : AbstractBaseChest<ChestController>
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

        var trig = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true)
        {
            EventDataList =
            {
                [0] = bananas
            }
        };

        Room.SendSyncEvent(trig);

        var rec = new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, player.GameObjectId.ToString(), true, 1f);

        Room.SendSyncEvent(rec);
    }
}
