using Server.Base.Network;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities;

public class LauncherControllerEntity : SyncedEntity<LauncherController>
{
    public float RotationSpeed => EntityData.RotationSpeed;
    public float MaxLaunchVelocity => EntityData.MaxLaunchVelocity;
    public float MinLaunchVelocity => EntityData.MinLaunchVelocity;

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var launchEvent = new Trigger_SyncEvent(Id.ToString(), Room.Time, true,
            player.GameObjectId.ToString(), true);

        netState.SendSyncEventToPlayer(launchEvent);
    }
}
