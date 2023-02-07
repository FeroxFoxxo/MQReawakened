using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class LauncherControllerEntity : SyncedEntity<LauncherController>
{
    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var launchEvent = new Trigger_SyncEvent(Id.ToString(), Level.Time, true,
            player.PlayerId.ToString(), true);

        netState.SendSyncEventToPlayer(launchEvent);
    }
}
