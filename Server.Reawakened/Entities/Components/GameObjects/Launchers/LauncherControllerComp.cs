using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Launchers;

public class LauncherControllerComp : Component<LauncherController>
{
    public float RotationSpeed => ComponentData.RotationSpeed;
    public float MaxLaunchVelocity => ComponentData.MaxLaunchVelocity;
    public float MinLaunchVelocity => ComponentData.MinLaunchVelocity;

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var launchEvent = new Trigger_SyncEvent(Id.ToString(), Room.Time, true,
            player.GameObjectId.ToString(), true);

        player.SendSyncEventToPlayer(launchEvent);
    }
}
